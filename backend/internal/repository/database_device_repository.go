package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
	"gorm.io/gorm"
)

type DatabaseDeviceRepository struct {
	db *gorm.DB
}

func NewDatabaseDeviceRepository(db *gorm.DB) DeviceRepository {
	return &DatabaseDeviceRepository{db}
}

func (r *DatabaseDeviceRepository) GetDevices(ctx context.Context, params api.GetDevicesParams) (*[]model.Device, error) {
    // codegen handles defaults
    limit := params.Limit.Value
	query := gorm.G[model.Device](r.db).Limit(limit)

	if v, ok := params.Name.Get(); ok {
		query = query.Where("name = ?", v)
	}
	if v, ok := params.GpuModel.Get(); ok {
		query = query.Where("gpu_model = ?", v)
	}
	if v, ok := params.MinVramMb.Get(); ok {
		query = query.Where("vram_mb >= ?", v)
	}
	if v, ok := params.MaxVramMb.Get(); ok {
		query = query.Where("vram_mb <= ?", v)
	}
	if v, ok := params.MinCudaCores.Get(); ok {
		query = query.Where("cuda_cores >= ?", v)
	}
	if v, ok := params.MaxCudaCores.Get(); ok {
		query = query.Where("cuda_cores <= ?", v)
	}
	if v, ok := params.MinPricePerHourUsdCents.Get(); ok {
		query = query.Where("price_per_hour_usd_cents >= ?", v)
	}
	if v, ok := params.MaxPricePerHourUsdCents.Get(); ok {
		query = query.Where("price_per_hour_usd_cents <= ?", v)
	}
	if v, ok := params.MinDriverVersion.Get(); ok {
		dv, err := utils.DriverVersionFromString(v)
		if err != nil {
			return nil, err
		}
		query = query.Where("driver_version_major > ?", dv.Major).Or(
			r.db.Where("driver_version_major = ?", dv.Major).Where("driver_version_minor > ?", dv.Minor),
		)
	}
	if v, ok := params.MaxDriverVersion.Get(); ok {
		dv, err := utils.DriverVersionFromString(v)
		if err != nil {
			return nil, err
		}
		query = query.Where("driver_version_major < ?", dv.Major).Or(
			r.db.Where("driver_version_major = ?", dv.Major).Where("driver_version_minor < ?", dv.Minor),
		)
	}
	if len(params.States) > 0 {
		query = query.Where("state IN ?", params.States)
	}

	devices, err := query.Order("ID").Find(ctx)
    return &devices, err
}

func (r *DatabaseDeviceRepository) GetDeviceById(ctx context.Context, id string) (*model.Device, error) {
	device, err := gorm.G[model.Device](r.db).Where("ID = ?", id).First(ctx)
	if err != nil {
		return nil, err
	}
	return &device, nil
}

func (r *DatabaseDeviceRepository) Transaction(fn func(repository DeviceRepository) error) error {
	return r.db.Transaction(func(tx *gorm.DB) error {
		return fn(&DatabaseDeviceRepository{tx})
	})
}

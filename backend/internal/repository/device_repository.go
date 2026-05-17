package repository

import (
	"context"

	"github.com/go-faster/errors"
	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
	"gorm.io/gorm"
)

type DeviceRepository interface {
	GetDevices(ctx context.Context, params api.GetDevicesParams) ([]model.Device, error)
	GetDeviceById(ctx context.Context, id string) (*model.Device, error)
	GetDevicesForUser(ctx context.Context, userId uint, params api.GetDevicesParams) ([]model.Device, error)
	AddDevice(ctx context.Context, device *model.Device) error
	UpdateDevice(ctx context.Context, device *model.Device) error
}

type deviceRepository struct {
	db *gorm.DB
}

func (r *deviceRepository) queryFromParams(params api.GetDevicesParams) (gorm.ChainInterface[model.Device], error) {
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

	return query, nil
}

func (r *deviceRepository) GetDevices(ctx context.Context, params api.GetDevicesParams) ([]model.Device, error) {
	query, err := r.queryFromParams(params)
	if err != nil {
		return nil, err
	}

	return query.Order("ID").Find(ctx)
}

func (r *deviceRepository) GetDeviceById(ctx context.Context, id string) (*model.Device, error) {
	device, err := gorm.G[model.Device](r.db).Where("ID = ?", id).First(ctx)
	if err != nil {
		return nil, err
	}
	return &device, nil
}

func (r *deviceRepository) GetDevicesForUser(ctx context.Context, userId uint, params api.GetDevicesParams) ([]model.Device, error) {
	query, err := r.queryFromParams(params)
	if err != nil {
		return nil, err
	}
	query = query.Where("user_id = ?", userId)

	return query.Order("ID").Find(ctx)
}

func (r *deviceRepository) AddDevice(ctx context.Context, device *model.Device) error {
	return gorm.G[model.Device](r.db).Create(ctx, device)
}

func (r *deviceRepository) UpdateDevice(ctx context.Context, device *model.Device) error {
	rowsAffected, err := gorm.G[model.Device](r.db).Where("id = ?", device.ID).Updates(ctx, *device)
	if err != nil {
		return err
	}
	if rowsAffected != 1 {
		return errors.New("affected rows is not equal to 1")
	}
	return nil
}

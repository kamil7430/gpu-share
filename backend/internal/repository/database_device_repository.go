package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/gorm"
)

type DatabaseDeviceRepository struct {
	db *gorm.DB
}

func NewDatabaseDeviceRepository(db *gorm.DB) DeviceRepository {
	return &DatabaseDeviceRepository{db}
}

func (r *DatabaseDeviceRepository) GetDevices(ctx context.Context, params api.GetDevicesParams) (*[]model.Device, error) {
	limit := 25
	if v, ok := params.Limit.Get(); ok {
		limit = v
	}
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
	if v, ok := params.MinPricePerHourUsd.Get(); ok {
		query = query.Where("price_per_hour_usd >= ?", v)
	}
	if v, ok := params.MaxPricePerHourUsd.Get(); ok {
		query = query.Where("price_per_hour_usd <= ?", v)
	}
	if v, ok := params.MinDriverVersion.Get(); ok {
		_ = v // TODO
	}
	if v, ok := params.MaxDriverVersion.Get(); ok {
		_ = v // TODO
	}
	if len(params.States) > 0 {
		query = query.Where("state IN ?", params.States)
	}

	devices, err := query.Order("ID").Find(ctx)
	if err != nil {
		return nil, err
	}
	return &devices, nil
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

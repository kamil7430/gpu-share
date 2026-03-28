package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/gorm"
)

type DatabaseDeviceRepository struct {
	db  *gorm.DB
	ctx context.Context
}

func (r *DatabaseDeviceRepository) GetDeviceById(id int) (*model.Device, error) {
	device, err := gorm.G[model.Device](r.db).Where("ID = ?", id).First(r.ctx)
	if err != nil {
		return nil, err
	}
	return &device, nil
}

func (r *DatabaseDeviceRepository) Transaction(fn func(repository DeviceRepository) error) error {
	return r.db.Transaction(func(tx *gorm.DB) error {
		return fn(&DatabaseDeviceRepository{tx, r.ctx})
	})
}

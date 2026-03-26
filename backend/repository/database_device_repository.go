package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/model"
	"gorm.io/gorm"
)

type DatabaseDeviceRepository struct {
	db *gorm.DB
}

func (d *DatabaseDeviceRepository) GetDeviceById(ctx context.Context, id int) (*model.Device, error) {
	device, err := gorm.G[model.Device](d.db).Where("ID = ?", id).First(ctx)
	if err != nil {
		return nil, err
	}
	return &device, nil
}

func (d *DatabaseDeviceRepository) Transaction(fn func(repository DeviceRepository) error) error {
	return d.db.Transaction(func(tx *gorm.DB) error {
		return fn(&DatabaseDeviceRepository{tx})
	})
}

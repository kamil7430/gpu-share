package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/model"
)

type DeviceRepository interface {
	GetDeviceById(ctx context.Context, id int) (*model.Device, error)
	Transaction(fn func(repository DeviceRepository) error) error
}

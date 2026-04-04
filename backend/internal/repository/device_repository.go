package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
)

type DeviceRepository interface {
	GetDevices(ctx context.Context, params api.GetDevicesParams) ([]*model.Device, error)
	GetDeviceById(ctx context.Context, id string) (*model.Device, error)
	Transaction(fn func(repository DeviceRepository) error) error
}

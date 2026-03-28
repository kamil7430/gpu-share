package repository

import (
	"github.com/kamil7430/gpu-share/backend/internal/model"
)

type DeviceRepository interface {
	GetDeviceById(id int) (*model.Device, error)
	Transaction(fn func(repository DeviceRepository) error) error
}

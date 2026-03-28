package repository

import "github.com/kamil7430/gpu-share/backend/internal/model"

type GpuRepository interface {
	GetDeviceStatusById(id string) (*model.DeviceStatus, error)
}

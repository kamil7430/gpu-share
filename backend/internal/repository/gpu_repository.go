package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/model"
)

type GpuRepository interface {
	GetDeviceStatusById(ctx context.Context, id string) (*model.DeviceStatus, error)
}

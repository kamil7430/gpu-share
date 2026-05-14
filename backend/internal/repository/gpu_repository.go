package repository

import (
	"context"
	"time"

	"github.com/kamil7430/gpu-share/backend/internal/model"
)

type GpuRepository interface {
	GetDeviceStatusById(ctx context.Context, id string) (*model.DeviceStatus, error)
	GetConnectionDetailsById(ctx context.Context, id string) (*model.ConnectionDetails, error)
}

type mockGpuRepository struct{}

func NewMockGpuRepository() GpuRepository {
	return &mockGpuRepository{}
}

func (m *mockGpuRepository) GetDeviceStatusById(ctx context.Context, id string) (*model.DeviceStatus, error) {
	return &model.DeviceStatus{
		TemperatureC:       69,
		UtilizationPercent: 69,
		MemoryUsedMb:       6969,
		LastHeartbeat:      time.Date(2005, 4, 2, 21, 37, 0, 0, time.UTC),
	}, nil
}

func (m *mockGpuRepository) GetConnectionDetailsById(ctx context.Context, id string) (*model.ConnectionDetails, error) {
	return &model.ConnectionDetails{
		Host:     "node-01.gpushare.net",
		Port:     "443",
		Protocol: "wws",
	}, nil
}

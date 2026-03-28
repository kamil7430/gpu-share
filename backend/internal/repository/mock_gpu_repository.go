package repository

import (
	"time"

	"github.com/kamil7430/gpu-share/backend/internal/model"
)

type MockGpuRepository struct{}

func (m *MockGpuRepository) GetDeviceStatusById(id int) (*model.DeviceStatus, error) {
	return &model.DeviceStatus{
		TemperatureC:       69,
		UtilizationPercent: 69,
		MemoryUsedMb:       6969,
		LastHeartbeat:      time.Date(2005, 4, 2, 21, 37, 0, 0, time.UTC),
	}, nil
}

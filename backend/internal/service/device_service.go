package service

import (
	"context"
	"errors"
	"strconv"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"gorm.io/gorm"
)

type DeviceService struct {
	dr repository.DeviceRepository
	gr repository.GpuRepository
}

func NewDeviceService(dr repository.DeviceRepository, gr repository.GpuRepository) DeviceService {
	return DeviceService{dr, gr}
}

func (s *DeviceService) GetDevices(ctx context.Context, params api.GetDevicesParams) (api.GetDevicesRes, error) {
	panic("todo")
}

func (s *DeviceService) GetDeviceStatus(ctx context.Context, params api.GetDeviceStatusParams) (api.GetDeviceStatusRes, error) {
	device, err := s.dr.GetDeviceById(ctx, params.DeviceId)
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return &api.GetDeviceStatusNotFound{}, nil
		}
		return nil, err
	}

	status, err := s.gr.GetDeviceStatusById(ctx, params.DeviceId)
	if err != nil {
		return nil, err
	}

	return &api.DeviceStatus{
		DeviceId:           strconv.Itoa(int(device.ID)),
		State:              device.State,
		TemperatureC:       status.TemperatureC,
		UtilizationPercent: status.UtilizationPercent,
		MemoryUsedMb:       status.MemoryUsedMb,
		LastHeartbeat:      status.LastHeartbeat,
	}, nil
}

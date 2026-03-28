package service

import (
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/service/responses"
)

type DeviceService struct {
	dr repository.DeviceRepository
	gr repository.GpuRepository
}

func NewDeviceService(dr repository.DeviceRepository, gr repository.GpuRepository) DeviceService {
	return DeviceService{dr, gr}
}

func (s *DeviceService) GetDeviceStatusById(id int) (*responses.DeviceStatusResponse, error) {
	device, err := s.dr.GetDeviceById(id)
	if err != nil {
		return nil, err
	}

	status, err := s.gr.GetDeviceStatusById(id)
	if err != nil {
		return nil, err
	}

	return &responses.DeviceStatusResponse{
		DeviceId:           device.ID,
		State:              device.State,
		TemperatureC:       status.TemperatureC,
		UtilizationPercent: status.UtilizationPercent,
		MemoryUsedMb:       status.MemoryUsedMb,
		LastHeartbeat:      status.LastHeartbeat,
	}, nil
}

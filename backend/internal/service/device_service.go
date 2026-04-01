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

var _ api.StrictServerInterface = (*DeviceService)(nil)

func NewDeviceService(dr repository.DeviceRepository, gr repository.GpuRepository) DeviceService {
	return DeviceService{dr, gr}
}

func (s *DeviceService) GetDeviceStatus(ctx context.Context, ro api.GetDeviceStatusRequestObject) (api.GetDeviceStatusResponseObject, error) {
	device, err := s.dr.GetDeviceById(ro.Id)
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return api.GetDeviceStatus404Response{}, nil
		}
		return nil, err
	}

	status, err := s.gr.GetDeviceStatusById(ro.Id)
	if err != nil {
		return nil, err
	}

	return api.GetDeviceStatus200JSONResponse(api.DeviceStatus{
		DeviceId:           strconv.Itoa(int(device.ID)),
		State:              device.State,
		TemperatureC:       status.TemperatureC,
		UtilizationPercent: status.UtilizationPercent,
		MemoryUsedMb:       status.MemoryUsedMb,
		LastHeartbeat:      status.LastHeartbeat,
	}), nil
}

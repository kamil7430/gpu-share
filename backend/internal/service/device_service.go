package service

import (
	"context"
	"strconv"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
)

type DeviceService struct {
	dr repository.DeviceRepository
	gr repository.GpuRepository
}

var _ api.StrictServerInterface = (*DeviceService)(nil)

func NewDeviceService(dr repository.DeviceRepository, gr repository.GpuRepository) DeviceService {
	return DeviceService{dr, gr}
}

func (s *DeviceService) GetDevice(ctx context.Context, ro api.GetDeviceRequestObject) (api.GetDeviceResponseObject, error) {
	return api.GetDevice200JSONResponse(api.Device{
		Id:   "aa",
		Name: "bb",
	}), nil
	device, err := s.dr.GetDeviceById(ro.Id)
	if err != nil {
		return nil, err
	}

	status, err := s.gr.GetDeviceStatusById(ro.Id)
	_ = status // <3 go
	if err != nil {
		return nil, err
	}

	return api.GetDevice200JSONResponse(api.Device{
		Id:   strconv.Itoa(int(device.ID)),
		Name: device.Name,
	}), nil

	// return &responses.DeviceStatusResponse{
	// 	DeviceId:           strconv.Itoa(int(device.ID)),
	// 	State:              device.State,
	// 	TemperatureC:       status.TemperatureC,
	// 	UtilizationPercent: status.UtilizationPercent,
	// 	MemoryUsedMb:       status.MemoryUsedMb,
	// 	LastHeartbeat:      status.LastHeartbeat,
	// }, nil
}

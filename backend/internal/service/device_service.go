package service

import (
	"context"
	"errors"
	"log"
	"strconv"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
	ht "github.com/ogen-go/ogen/http"
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
	// See `/contract/openapi/paths/api/devices/devices.yaml` for more information.
	// In particular regarding filters values constraints.
	if minVramMb, ok := params.MinVramMb.Get(); ok {
		if maxVramMb, ok := params.MaxVramMb.Get(); ok && minVramMb > maxVramMb {
			return &api.GetDevicesBadRequest{}, nil
		}
	}

	if minCudaCores, ok := params.MinCudaCores.Get(); ok {
		if maxCudaCores, ok := params.MaxCudaCores.Get(); ok && minCudaCores > maxCudaCores {
			return &api.GetDevicesBadRequest{}, nil
		}
	}

	if minPricePerHour, ok := params.MinPricePerHourUsdCents.Get(); ok {
		if maxPricePerHour, ok := params.MaxPricePerHourUsdCents.Get(); ok && minPricePerHour > maxPricePerHour {
			return &api.GetDevicesBadRequest{}, nil
		}
	}

	minDriverVersion, err := utils.DriverVersionFromString(params.MinDriverVersion.Or("0.0"))
	if err != nil {
		return &api.GetDevicesBadRequest{}, err
	}
	if v, ok := params.MaxDriverVersion.Get(); ok {
		maxDriverVersion, err := utils.DriverVersionFromString(v)
		if err != nil || minDriverVersion.Compare(maxDriverVersion) > 0 {
			return &api.GetDevicesBadRequest{}, err
		}
	}

	devices, err := s.dr.GetDevices(ctx, params)
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return &api.GetDevicesNotFound{}, nil
		}
		return nil, err
	}
	if len(*devices) <= 0 {
		return &api.GetDevicesNotFound{}, nil
	}

	var result api.GetDevicesOKApplicationJSON

	for _, dev := range *devices {
		dv, err := utils.NewDriverVersion(dev.DriverVersionMajor, dev.DriverVersionMinor)
		if err != nil {
			log.Fatal(err) // should be unreachable
		}
		result = append(result, api.Device{
			DeviceId:             strconv.Itoa(int(dev.ID)),
			Name:                 dev.Name,
			GpuModel:             dev.GpuModel,
			VramMb:               dev.VramMb,
			CudaCores:            dev.CudaCores,
			PricePerHourUsdCents: dev.PricePerHourUsdCents,
			DriverVersion:        dv.String(),
			State:                dev.State,
		})
	}

	return &result, nil
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

func (s *DeviceService) AddDevice(ctx context.Context, req *api.Device) (api.AddDeviceRes, error) {
	return nil, ht.ErrNotImplemented
}

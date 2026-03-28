package service

import "github.com/kamil7430/gpu-share/repository"

type DeviceService struct {
	r repository.DeviceRepository
}


func (s *DeviceService) GetDeviceStatusById(id int)

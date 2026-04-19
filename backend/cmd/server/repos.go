package server

import "github.com/kamil7430/gpu-share/backend/internal/repository"

type Repos struct {
	DeviceRepo repository.DeviceRepository
	GpuRepo    repository.GpuRepository
	UserRepo   repository.UserRepository
}

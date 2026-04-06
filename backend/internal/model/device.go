package model

import (
	"github.com/kamil7430/gpu-share/backend/internal/api"
	"gorm.io/gorm"
)

type Device struct {
	gorm.Model
	Name                 string
	GpuModel             string
	VramMb               int
	CudaCores            int
	PricePerHourUsdCents int
	DriverVersionMajor   int
	DriverVersionMinor   int
	State                api.State
}

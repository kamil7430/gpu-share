package model

import "gorm.io/gorm"

type Device struct {
	gorm.Model
	Name            string      `json:"name"`
	GpuModel        string      `json:"gpu_model"`
	VramMb          int         `json:"vram_mb"`
	CudaCores       int         `json:"cuda_cores"`
	PricePerHourUsd float32     `json:"price_per_hour_usd"`
	DriverVersion   string      `json:"driver_version"`
	State           DeviceState `json:"state"`
}

package model

import "gorm.io/gorm"

type Device struct {
	gorm.Model
	Name            string
	GpuModel        string
	VramMb          int
	CudaCores       int
	PricePerHourUsd float32
	DriverVersion   string
	State           DeviceState
}

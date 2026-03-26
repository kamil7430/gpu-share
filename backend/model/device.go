package model

import "gorm.io/gorm"

type Device struct {
	gorm.Model
	ModelName       string
	VramMb          int
	PricePerHourUsd float32
	State           DeviceState
}

package model

import "gorm.io/gorm"

type Order struct {
	gorm.Model
	DockerImage   string
	DurationHours float32
	UserID        uint
	DeviceID      uint
}

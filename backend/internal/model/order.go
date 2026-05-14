package model

import (
	"github.com/kamil7430/gpu-share/backend/internal/api"
	"gorm.io/gorm"
)

type Order struct {
	gorm.Model
	DockerImage     string
	DurationHours   float32
	RentalStatus    api.RentalStatus
	RentalCostCents int
	UserID          uint
	DeviceID        uint
}

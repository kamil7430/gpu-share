package model

import "time"

type DeviceStatus struct {
	TemperatureC       int
	UtilizationPercent int
	MemoryUsedMb       int
	LastHeartbeat      time.Time
}

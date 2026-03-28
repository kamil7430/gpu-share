package responses

import (
	"time"

	"github.com/kamil7430/gpu-share/backend/internal/model"
)

type DeviceStatusResponse struct {
	DeviceId           string            `json:"device_id"`
	State              model.DeviceState `json:"state"`
	TemperatureC       int               `json:"temperature_c"`
	UtilizationPercent int               `json:"utilization_percent"`
	MemoryUsedMb       int               `json:"memory_used_mb"`
	LastHeartbeat      time.Time         `json:"last_heartbeat"`
}

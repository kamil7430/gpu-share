package service

import (
	"context"
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/backend/internal"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/stretchr/testify/require"
)

func TestDatabaseDeviceRepository(t *testing.T) {
	db, err := internal.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	s := NewDeviceService(
		repository.NewDatabaseDeviceRepository(tx, context.Background()),
		repository.NewMockGpuRepository(),
	)

	deviceId := "2137"

	resetDbContent := func() {
		tx.Exec("TRUNCATE TABLE devices;")
		tx.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd, driver_version, state) " +
			"VALUES ('" + deviceId + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '15.99', '595.97', 'UNAVAILABLE');")
	}

	t.Run("get device status", func(t *testing.T) {
		resetDbContent()
		device, err := s.GetDeviceStatusById(deviceId)
		require.NoError(t, err)
		require.NotNil(t, device)
		require.Equal(t, deviceId, device.DeviceId)
		require.Equal(t, model.Unavailable, device.State)
		require.Equal(t, 69, device.TemperatureC)
		require.Equal(t, 69, device.UtilizationPercent)
		require.Equal(t, 6969, device.MemoryUsedMb)
		require.Equal(t, time.Date(2005, 4, 2, 21, 37, 0, 0, time.UTC), device.LastHeartbeat)
	})
}

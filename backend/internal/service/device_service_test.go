package service

import (
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/backend/internal"
	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/stretchr/testify/require"
)

func TestDatabaseDeviceRepository(t *testing.T) {
	db, err := internal.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	s := NewDeviceService(
		repository.NewDatabaseDeviceRepository(tx, t.Context()),
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
		req := api.GetDeviceStatusParams{DeviceId: deviceId}
		device, err := s.GetDeviceStatus(t.Context(), req)
		require.NoError(t, err)
		require.NotNil(t, device)

		res, ok := device.(*api.DeviceStatus)
		require.True(t, ok)

		require.Equal(t, deviceId, res.DeviceId)
		require.Equal(t, api.DeviceStatusStateUNAVAILABLE, res.State)
		require.Equal(t, 69, res.TemperatureC)
		require.Equal(t, 69, res.UtilizationPercent)
		require.Equal(t, 6969, res.MemoryUsedMb)
		require.Equal(t, time.Date(2005, 4, 2, 21, 37, 0, 0, time.UTC), res.LastHeartbeat)
	})
}

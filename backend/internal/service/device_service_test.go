package service

import (
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
	"github.com/stretchr/testify/require"
)

func TestDeviceService(t *testing.T) {
	db, err := utils.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	s := NewDeviceService(
		repository.NewDatabaseDeviceRepository(tx),
		repository.NewMockGpuRepository(),
	)

	deviceId := "2137"

	resetDbContent := func() {
		tx.Exec("TRUNCATE TABLE devices;")
		tx.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd_cents, driver_version_major, driver_version_minor, state) " +
			"VALUES ('" + deviceId + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'UNAVAILABLE'), " +
			"('2138', 'TestCard2', 'NVIDIA GeForce RTX 3050', '8192', '2560', '2599', '595', '97', 'AVAILABLE'), " +
			"('2139', 'TestCard3', 'NVIDIA GeForce GTX 1050 Ti', '4096', '768', '699', '582', '28', 'AVAILABLE');")
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
		require.Equal(t, api.StateUNAVAILABLE, res.State)
		require.Equal(t, 69, res.TemperatureC)
		require.Equal(t, 69, res.UtilizationPercent)
		require.Equal(t, 6969, res.MemoryUsedMb)
		require.Equal(t, time.Date(2005, 4, 2, 21, 37, 0, 0, time.UTC), res.LastHeartbeat)
	})

	t.Run("get nonexistent device status", func(t *testing.T) {
		resetDbContent()
		req := api.GetDeviceStatusParams{DeviceId: "6969"}
		device, err := s.GetDeviceStatus(t.Context(), req)
		require.NoError(t, err)
		require.NotNil(t, device)

		_, ok := device.(*api.GetDeviceStatusNotFound)
		require.True(t, ok)
	})
}

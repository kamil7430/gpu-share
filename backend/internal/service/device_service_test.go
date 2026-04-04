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
		repository.NewDatabaseDeviceRepository(tx),
		repository.NewMockGpuRepository(),
	)

	deviceId := "2137"

	resetDbContent := func() {
		tx.Exec("TRUNCATE TABLE devices;")
		tx.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd, driver_version, state) " +
			"VALUES ('" + deviceId + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '15.99', '595.97', 'UNAVAILABLE'), " +
			"('2138', 'TestCard2', 'NVIDIA GeForce RTX 3050', '8192', '2560', '25.99', '595.97', 'AVAILABLE'), " +
			"('2139', 'TestCard3', 'NVIDIA GeForce GTX 1050 Ti', '4096', '768', '6.99', '582.28', 'AVAILABLE');")
	}

	t.Run("fetch by name", func(t *testing.T) {
		resetDbContent()
		req := api.GetDevicesParams{Name: api.OptString{
			Value: "TestCard2",
			Set:   true,
		}}
		devices, err := s.GetDevices(t.Context(), req)
		require.NoError(t, err)
		require.NotNil(t, devices)

		res, ok := devices.(*api.GetDevicesOKApplicationJSON)
		require.True(t, ok)
		require.Len(t, *res, 1)

		dev := (*res)[0]
		require.Equal(t, "2138", dev.DeviceId)
		require.Equal(t, "TestCard2", dev.Name)
		require.Equal(t, "NVIDIA GeForce RTX 3050", dev.GpuModel)
		require.Equal(t, 8192, dev.VramMb)
		require.Equal(t, 2560, dev.CudaCores)
		require.Equal(t, float32(25.99), dev.PricePerHourUsd)
		require.Equal(t, "595.97", dev.DriverVersion)
		require.Equal(t, api.StateAVAILABLE, dev.State)
	})

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

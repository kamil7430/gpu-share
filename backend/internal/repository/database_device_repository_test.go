package repository

import (
	"context"
	"strconv"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/stretchr/testify/require"
)

func TestDatabaseDeviceRepository(t *testing.T) {
	db, err := internal.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	r := NewDatabaseDeviceRepository(
		tx,
		context.Background(),
	)

	deviceId := 2137

	resetDbContent := func() {
		tx.Exec("TRUNCATE TABLE devices;")
		tx.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd, driver_version, state) " +
			"VALUES ('" + strconv.Itoa(deviceId) + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '15.99', '595.97', '0');")
	}

	t.Run("get device", func(t *testing.T) {
		resetDbContent()
		device, err := r.GetDeviceById(deviceId)
		require.NoError(t, err)
		require.NotNil(t, device)
		require.Equal(t, "TestCard", device.Name)
		require.Equal(t, "NVIDIA GeForce RTX 3050", device.GpuModel)
		require.Equal(t, 8192, device.VramMb)
		require.Equal(t, 2560, device.CudaCores)
		require.Equal(t, float32(15.99), device.PricePerHourUsd)
		require.Equal(t, "595.97", device.DriverVersion)
		require.Equal(t, model.Unavailable, device.State)
	})
}

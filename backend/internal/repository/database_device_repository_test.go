package repository

import (
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal"
	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func TestDatabaseDeviceRepository(t *testing.T) {
	db, err := internal.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	r := NewDatabaseDeviceRepository(tx)

	deviceId := "2137"

	resetDbContent := func() {
		tx.Exec("TRUNCATE TABLE devices;")
		tx.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd, driver_version, state) " +
			"VALUES ('" + deviceId + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '15.99', '595.97', 'UNAVAILABLE');")
	}

	t.Run("get device", func(t *testing.T) {
		resetDbContent()
		device, err := r.GetDeviceById(t.Context(), deviceId)
		require.NoError(t, err)
		require.NotNil(t, device)
		require.Equal(t, "TestCard", device.Name)
		require.Equal(t, "NVIDIA GeForce RTX 3050", device.GpuModel)
		require.Equal(t, 8192, device.VramMb)
		require.Equal(t, 2560, device.CudaCores)
		require.Equal(t, float32(15.99), device.PricePerHourUsd)
		require.Equal(t, "595.97", device.DriverVersion)
		require.Equal(t, api.StateUNAVAILABLE, device.State)
	})

	t.Run("get nonexistent device", func(t *testing.T) {
		resetDbContent()
		device, err := r.GetDeviceById(t.Context(), "6969")
		require.Nil(t, device)
		require.ErrorIs(t, err, gorm.ErrRecordNotFound)
	})
}

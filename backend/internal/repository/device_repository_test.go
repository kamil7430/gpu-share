package repository

import (
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func TestDatabaseDeviceRepository(t *testing.T) {
	db, err := utils.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	r := NewStore(tx).Devices()

	deviceId := "2137"

	resetDbContent := func() {
		tx.Exec("TRUNCATE TABLE users, orders, devices, reviews;")
		tx.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd_cents, driver_version_major, driver_version_minor, state) " +
			"VALUES ('" + deviceId + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'UNAVAILABLE');")
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
		require.Equal(t, 1599, device.PricePerHourUsdCents)
		require.Equal(t, 595, device.DriverVersionMajor)
		require.Equal(t, 97, device.DriverVersionMinor)
		require.Equal(t, api.StateUNAVAILABLE, device.State)
	})

	t.Run("get nonexistent device", func(t *testing.T) {
		resetDbContent()
		_, err := r.GetDeviceById(t.Context(), "6969")
		require.ErrorIs(t, err, gorm.ErrRecordNotFound)
	})

	t.Run("get devices by ids preserves order", func(t *testing.T) {
		resetDbContent()

		tx.Exec(`
		INSERT INTO devices(
			id, name, gpu_model, vram_mb, cuda_cores,
			price_per_hour_usd_cents, driver_version_major,
			driver_version_minor, state
		) VALUES
			(1001, 'Card1', 'GPU1', 4096, 1000, 100, 1, 0, 'AVAILABLE'),
			(1002, 'Card2', 'GPU2', 8192, 2000, 200, 1, 0, 'AVAILABLE'),
			(1003, 'Card3', 'GPU3', 12288, 3000, 300, 1, 0, 'AVAILABLE')
		`)

		devices, err := r.GetDevicesByIds(t.Context(), []uint{1003, 1001, 1002})
		require.NoError(t, err)
		require.Len(t, devices, 3)

		require.EqualValues(t, 1003, devices[0].ID)
		require.Equal(t, "Card3", devices[0].Name)

		require.EqualValues(t, 1001, devices[1].ID)
		require.Equal(t, "Card1", devices[1].Name)

		require.EqualValues(t, 1002, devices[2].ID)
		require.Equal(t, "Card2", devices[2].Name)
	})

	t.Run("get devices by ids ignores missing ids", func(t *testing.T) {
		resetDbContent()

		devices, err := r.GetDevicesByIds(t.Context(), []uint{2137, 999999})
		require.NoError(t, err)
		require.Len(t, devices, 1)

		require.EqualValues(t, 2137, devices[0].ID)
		require.Equal(t, "TestCard", devices[0].Name)
	})

	t.Run("get devices by ids empty input", func(t *testing.T) {
		resetDbContent()

		devices, err := r.GetDevicesByIds(t.Context(), []uint{})
		require.NoError(t, err)
		require.Empty(t, devices)
	})
}

package repository

import (
	"context"
	"strconv"
	"testing"

	"github.com/kamil7430/gpu-share/model"
	"github.com/stretchr/testify/require"
	"github.com/testcontainers/testcontainers-go"
	"github.com/testcontainers/testcontainers-go/modules/postgres"
	gormpostgres "gorm.io/driver/postgres"
	"gorm.io/gorm"
)

func TestDatabaseDeviceRepository(t *testing.T) {
	ctx := context.Background()
	dbName := "deviceRepositoryTests"
	dbUser := "user"
	dbPassword := "password"

	ctr, err := postgres.Run(
		ctx,
		"postgres:16-alpine",
		postgres.WithDatabase(dbName),
		postgres.WithUsername(dbUser),
		postgres.WithPassword(dbPassword),
		postgres.BasicWaitStrategies(),
		postgres.WithSQLDriver("pgx"),
	)
	testcontainers.CleanupContainer(t, ctr)
	require.NoError(t, err)

	err = ctr.Snapshot(ctx)
	require.NoError(t, err)

	dbURL, err := ctr.ConnectionString(ctx)
	require.NoError(t, err)

	db, err := gorm.Open(gormpostgres.Open(dbURL), &gorm.Config{})
	require.NoError(t, err)

	err = db.AutoMigrate(&model.Device{})
	require.NoError(t, err)

	d := DatabaseDeviceRepository{
		db,
	}
	deviceId := 2137

	resetDbContent := func() {
		db.Exec("TRUNCATE TABLE devices")
		db.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd, driver_version, state) " +
			"VALUES ('" + strconv.Itoa(deviceId) + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '15.99', '595.97', '0')")
	}

	t.Run("get device", func(t *testing.T) {
		resetDbContent()
		device, err := d.GetDeviceById(ctx, deviceId)
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

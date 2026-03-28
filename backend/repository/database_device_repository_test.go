package repository

import (
	"context"
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
	dbname := "deviceRepositoryTests"
	dbuser := "user"
	dbpassword := "password"

	ctr, err := postgres.Run(
		ctx,
		"postgres:16-alpine",
		postgres.WithDatabase(dbname),
		postgres.WithUsername(dbuser),
		postgres.WithPassword(dbpassword),
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

	d := DatabaseDeviceRepository{}

	resetDbContent := func() {
		db.Exec("TRUNCATE TABLE devices")
		db.Exec("INSERT INTO devices(name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd, driver_version, state) " +
			"VALUES ('TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '15.99', '595.97', '0')")
	}

	t.Run("get device", func(t *testing.T) {
		resetDbContent()
		device, err := d.GetDeviceById(ctx, 0)
		require.NoError(t, err)
		require.Equal(t, "TestCard", device.Name)
		require.Equal(t, "NVIDIA GeForce RTX 3050", device.GpuModel)
		require.Equal(t, 8192, device.VramMb)
		require.Equal(t, 2560, device.CudaCores)
		require.Equal(t, 15.99, device.PricePerHourUsd)
		require.Equal(t, "595.97", device.DriverVersion)
		require.Equal(t, model.Unavailable, device.State)
	})
}

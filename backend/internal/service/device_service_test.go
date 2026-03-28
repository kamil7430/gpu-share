package service

import (
	"context"
	"strconv"
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/stretchr/testify/require"
	"github.com/testcontainers/testcontainers-go"
	"github.com/testcontainers/testcontainers-go/modules/postgres"
	gormpostgres "gorm.io/driver/postgres"
	"gorm.io/gorm"
)

func TestDatabaseDeviceRepository(t *testing.T) {
	ctx := context.Background()
	dbName := "deviceServiceTests"
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

	s := NewDeviceService(
		repository.NewDatabaseDeviceRepository(db, context.Background()),
		repository.NewMockGpuRepository(),
	)

	deviceId := 2137

	resetDbContent := func() {
		db.Exec("TRUNCATE TABLE devices")
		db.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd, driver_version, state) " +
			"VALUES ('" + strconv.Itoa(deviceId) + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '15.99', '595.97', '0')")
	}

	t.Run("get device status", func(t *testing.T) {
		resetDbContent()
		device, err := s.GetDeviceStatusById(deviceId)
		require.NoError(t, err)
		require.NotNil(t, device)
		require.Equal(t, uint(deviceId), device.DeviceId)
		require.Equal(t, model.Unavailable, device.State)
		require.Equal(t, 69, device.TemperatureC)
		require.Equal(t, 69, device.UtilizationPercent)
		require.Equal(t, 6969, device.MemoryUsedMb)
		require.Equal(t, time.Date(2005, 4, 2, 21, 37, 0, 0, time.UTC), device.LastHeartbeat)
	})
}

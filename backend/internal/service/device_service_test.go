package service

import (
	"math"
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

	checkTestCardInfo := func(dev api.Device) {
		require.Equal(t, deviceId, dev.DeviceId)
		require.Equal(t, "TestCard", dev.Name)
		require.Equal(t, "NVIDIA GeForce RTX 3050", dev.GpuModel)
		require.Equal(t, 8192, dev.VramMb)
		require.Equal(t, 2560, dev.CudaCores)
		require.LessOrEqual(t, math.Abs(15.99-dev.PricePerHourUsd), 0.01)
		require.Equal(t, "595.97", dev.DriverVersion)
		require.Equal(t, api.StateUNAVAILABLE, dev.State)
	}

	checkTestCard2Info := func(dev api.Device) {
		require.Equal(t, "2138", dev.DeviceId)
		require.Equal(t, "TestCard2", dev.Name)
		require.Equal(t, "NVIDIA GeForce RTX 3050", dev.GpuModel)
		require.Equal(t, 8192, dev.VramMb)
		require.Equal(t, 2560, dev.CudaCores)
		require.LessOrEqual(t, math.Abs(25.99-dev.PricePerHourUsd), 0.01)
		require.Equal(t, "595.97", dev.DriverVersion)
		require.Equal(t, api.StateAVAILABLE, dev.State)
	}

	checkTestCard3Info := func(dev api.Device) {
		require.Equal(t, "2139", dev.DeviceId)
		require.Equal(t, "TestCard3", dev.Name)
		require.Equal(t, "NVIDIA GeForce GTX 1050 Ti", dev.GpuModel)
		require.Equal(t, 4096, dev.VramMb)
		require.Equal(t, 768, dev.CudaCores)
		require.LessOrEqual(t, math.Abs(6.99-dev.PricePerHourUsd), 0.01)
		require.Equal(t, "582.28", dev.DriverVersion)
		require.Equal(t, api.StateAVAILABLE, dev.State)
	}

	getDevicesTestCase := func(req api.GetDevicesParams, checkFunctions ...func(dev api.Device)) {
		resetDbContent()
		devices, err := s.GetDevices(t.Context(), req)
		require.NoError(t, err)
		require.NotNil(t, devices)

		res, ok := devices.(*api.GetDevicesOKApplicationJSON)
		require.True(t, ok)
		require.Len(t, *res, len(checkFunctions))

		for i, checkFunction := range checkFunctions {
			checkFunction((*res)[i])
		}
	}

	t.Run("get devices -- no filter", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{},
			checkTestCardInfo,
			checkTestCard2Info,
			checkTestCard3Info,
		)
	})

	t.Run("get devices by name", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{Name: api.NewOptString("TestCard2")},
			checkTestCard2Info,
		)
	})

	t.Run("get devices by model", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{GpuModel: api.NewOptString("NVIDIA GeForce RTX 3050")},
			checkTestCardInfo,
			checkTestCard2Info,
		)
	})

	t.Run("get devices by minVramMb", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{MinVramMb: api.NewOptInt(5000)},
			checkTestCardInfo,
			checkTestCard2Info,
		)
	})

	t.Run("get devices by maxVramMb", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{MaxVramMb: api.NewOptInt(5000)},
			checkTestCard3Info,
		)
	})

	t.Run("get devices by minCudaCores", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{MinCudaCores: api.NewOptInt(1000)},
			checkTestCardInfo,
			checkTestCard2Info,
		)
	})

	t.Run("get devices by maxCudaCores", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{MaxCudaCores: api.NewOptInt(1000)},
			checkTestCard3Info,
		)
	})

	t.Run("get devices by minPricePerHourUsd", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{MinPricePerHourUsd: api.NewOptFloat64(20)},
			checkTestCard2Info,
		)
	})

	t.Run("get devices by maxPricePerHourUsd", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{MaxPricePerHourUsd: api.NewOptFloat64(20)},
			checkTestCardInfo,
			checkTestCard3Info,
		)
	})

	t.Run("get devices by minDriverVersion", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{MinDriverVersion: api.NewOptString("585")},
			checkTestCardInfo,
			checkTestCard2Info,
		)
	})

	t.Run("get devices by maxDriverVersion", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{MaxDriverVersion: api.NewOptString("585")},
			checkTestCard3Info,
		)
	})

	t.Run("get devices by states", func(t *testing.T) {
		getDevicesTestCase(api.GetDevicesParams{
			States: []api.State{api.StateRENTED, api.StateUNAVAILABLE},
		},
			checkTestCardInfo,
		)
	})

	getDevicesTestBadRequests := func(req api.GetDevicesParams) {
		resetDbContent()
		devices, err := s.GetDevices(t.Context(), req)
		require.NoError(t, err)
		require.Nil(t, devices)

		_, ok := devices.(*api.GetDevicesBadRequest)
		require.True(t, ok)
	}

	t.Run("get devices -- invalid minVramMb value", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{MinVramMb: api.NewOptInt(-5)})
	})

	t.Run("get devices -- invalid maxVramMb value", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{MaxVramMb: api.NewOptInt(-5)})
	})

	t.Run("get devices -- minVramMb > maxVramMb", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{
			MinVramMb: api.NewOptInt(1500),
			MaxVramMb: api.NewOptInt(1000),
		})
	})

	t.Run("get devices -- invalid minCudaCores value", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{MinCudaCores: api.NewOptInt(-5)})
	})

	t.Run("get devices -- invalid maxCudaCores value", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{MaxCudaCores: api.NewOptInt(-5)})
	})

	t.Run("get devices -- minCudaCores > maxCudaCores", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{
			MinCudaCores: api.NewOptInt(1500),
			MaxCudaCores: api.NewOptInt(1000),
		})
	})

	t.Run("get devices -- invalid minPricePerHourUsd", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{MinPricePerHourUsd: api.NewOptFloat64(-5)})
	})

	t.Run("get devices -- invalid maxPricePerHourUsd", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{MaxPricePerHourUsd: api.NewOptFloat64(-5)})
	})

	t.Run("get devices -- minPricePerHourUsd > maxPricePerHourUsd", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{
			MinPricePerHourUsd: api.NewOptFloat64(4.99),
			MaxPricePerHourUsd: api.NewOptFloat64(2.99),
		})
	})

	t.Run("get devices -- minDriverVersion > maxDriverVersion", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{
			MinDriverVersion: api.NewOptString("585.90"),
			MaxDriverVersion: api.NewOptString("587.12"),
		})
	})

	t.Run("get devices -- empty states array", func(t *testing.T) {
		getDevicesTestBadRequests(api.GetDevicesParams{States: []api.State{}})
	})

	getDevicesTestNotFound := func(req api.GetDevicesParams) {
		resetDbContent()
		devices, err := s.GetDevices(t.Context(), req)
		require.NoError(t, err)
		require.Nil(t, devices)

		_, ok := devices.(*api.GetDevicesNotFound)
		require.True(t, ok)
	}

	t.Run("get devices by name -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{Name: api.NewOptString("NONEXISTENT")})
	})

	t.Run("get devices by gpu model -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{GpuModel: api.NewOptString("NONEXISTENT")})
	})

	t.Run("get devices by minVram -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{MinVramMb: api.NewOptInt(50000)})
	})

	t.Run("get devices by maxVram -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{MaxVramMb: api.NewOptInt(50)})
	})

	t.Run("get devices by minCudaCores -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{MinCudaCores: api.NewOptInt(50000)})
	})

	t.Run("get devices by maxCudaCores -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{MaxCudaCores: api.NewOptInt(50)})
	})

	t.Run("get devices by minPricePerHour -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{MinPricePerHourUsd: api.NewOptFloat64(2000)})
	})

	t.Run("get devices by maxPricePerHour -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{MaxPricePerHourUsd: api.NewOptFloat64(0.50)})
	})

	t.Run("get devices by minDriverVersion -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{MinDriverVersion: api.NewOptString("800.88")})
	})

	t.Run("get devices by maxDriverVersion -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{MaxDriverVersion: api.NewOptString("1.1")})
	})

	t.Run("get devices by states -- not found", func(t *testing.T) {
		getDevicesTestNotFound(api.GetDevicesParams{
			States: []api.State{api.StateRENTED, api.StateREPORTED},
		})
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

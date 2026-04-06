package api_tests

import (
	"io"
	"net/http"
	"strings"
	"testing"

	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func testGetDeviceStatus(t *testing.T, db *gorm.DB, baseUrl string) {
	deviceId := "123"

	db.Exec("TRUNCATE TABLE devices;")
	db.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd_cents, driver_version_major, driver_version_minor, state) " +
		"VALUES ('" + deviceId + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE');")

	t.Run("get device status by id -- existent", func(t *testing.T) {
		resp, err := http.Get(baseUrl + "/api/devices/" + deviceId + "/status")
		require.NoError(t, err)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		expected := `{
			"deviceId": "123",
			"state": "AVAILABLE",
			"temperatureC": 69,
			"utilizationPercent": 69,
			"memoryUsedMb": 6969,
			"lastHeartbeat": "2005-04-02T21:37:00Z"
		}`

		require.JSONEq(t, expected, string(body))
	})

	t.Run("get device status by id -- nonexistent", func(t *testing.T) {
		resp, err := http.Get(baseUrl + "/api/devices/6969/status")
		require.NoError(t, err)
		defer resp.Body.Close()

		require.Equal(t, http.StatusNotFound, resp.StatusCode)
	})
}

func testGetDevices(t *testing.T, db *gorm.DB, baseUrl string) {
	resetDbContent := func() {
		db.Exec("TRUNCATE TABLE devices;")
		db.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd_cents, driver_version_major, driver_version_minor, state) " +
			"VALUES ('2137', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'UNAVAILABLE'), " +
			"('2138', 'TestCard2', 'NVIDIA GeForce RTX 3050', '8192', '2560', '2599', '595', '97', 'AVAILABLE'), " +
			"('2139', 'TestCard3', 'NVIDIA GeForce GTX 1050 Ti', '4096', '768', '699', '582', '28', 'AVAILABLE');")
	}

	testCardInfo := `{
		"deviceId": "2137",
		"name": "TestCard",
		"gpuModel": "NVIDIA GeForce RTX 3050",
		"vramMb": 8192,
		"cudaCores": 2560,
		"pricePerHourUsdCents": 1599,
		"driverVersion": "595.97",
		"state": "UNAVAILABLE"
	}`

	testCard2Info := `{
		"deviceId": "2138",
		"name": "TestCard2",
		"gpuModel": "NVIDIA GeForce RTX 3050",
		"vramMb": 8192,
		"cudaCores": 2560,
		"pricePerHourUsdCents": 2599,
		"driverVersion": "595.97",
		"state": "AVAILABLE"
	}`

	testCard3Info := `{
		"deviceId": "2139",
		"name": "TestCard3",
		"gpuModel": "NVIDIA GeForce GTX 1050 Ti",
		"vramMb": 4096,
		"cudaCores": 768,
		"pricePerHourUsdCents": 699,
		"driverVersion": "582.28",
		"state": "AVAILABLE"
	}`

	getDevicesTestCase := func(params string, expectedDevices ...string) {
		resetDbContent()
		resp, err := http.Get(baseUrl + "/api/devices?" + params)
		require.NoError(t, err)
		require.NotNil(t, resp)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		var expected strings.Builder
		expected.WriteString("[")
		expected.WriteString(strings.Join(expectedDevices, ","))
		expected.WriteString("]")

		require.JSONEq(t, expected.String(), string(body))
	}

	t.Run("get devices -- no filter", func(t *testing.T) {
		getDevicesTestCase("",
			testCardInfo,
			testCard2Info,
			testCard3Info,
		)
	})

	t.Run("get devices by name", func(t *testing.T) {
		getDevicesTestCase("name=TestCard2",
			testCard2Info,
		)
	})

	t.Run("get devices by model", func(t *testing.T) {
		getDevicesTestCase("gpuModel=NVIDIA%20GeForce%20RTX%203050",
			testCardInfo,
			testCard2Info,
		)
	})

	t.Run("get devices by minVramMb", func(t *testing.T) {
		getDevicesTestCase("minVramMb=5000",
			testCardInfo,
			testCard2Info,
		)
	})

	t.Run("get devices by maxVramMb", func(t *testing.T) {
		getDevicesTestCase("maxVramMb=5000",
			testCard3Info,
		)
	})

	t.Run("get devices by minCudaCores", func(t *testing.T) {
		getDevicesTestCase("minCudaCores=1000",
			testCardInfo,
			testCard2Info,
		)
	})

	t.Run("get devices by maxCudaCores", func(t *testing.T) {
		getDevicesTestCase("maxCudaCores=1000",
			testCard3Info,
		)
	})

	t.Run("get devices by minPricePerHourUsdCents", func(t *testing.T) {
		getDevicesTestCase("minPricePerHourUsdCents=2000",
			testCard2Info,
		)
	})

	t.Run("get devices by maxPricePerHourUsdCents", func(t *testing.T) {
		getDevicesTestCase("maxPricePerHourUsdCents=2000",
			testCardInfo,
			testCard3Info,
		)
	})

	t.Run("get devices by minDriverVersion", func(t *testing.T) {
		getDevicesTestCase("minDriverVersion=585.0",
			testCardInfo,
			testCard2Info,
		)
	})

	t.Run("get devices by maxDriverVersion", func(t *testing.T) {
		getDevicesTestCase("maxDriverVersion=585.0",
			testCard3Info,
		)
	})

	t.Run("get devices by states", func(t *testing.T) {
		getDevicesTestCase("states=RENTED&states=UNAVAILABLE",
			testCardInfo,
		)
	})

	getDevicesTestBadRequests := func(params string) {
		resetDbContent()
		resp, err := http.Get(baseUrl + "/api/devices?" + params)
		require.NoError(t, err)
		require.NotNil(t, resp)
		defer resp.Body.Close()

		require.Equal(t, http.StatusBadRequest, resp.StatusCode)
	}

	t.Run("get devices -- invalid minVramMb value", func(t *testing.T) {
		getDevicesTestBadRequests("minVramMb=-5")
	})

	t.Run("get devices -- invalid maxVramMb value", func(t *testing.T) {
		getDevicesTestBadRequests("maxVramMb=-5")
	})

	t.Run("get devices -- minVramMb > maxVramMb", func(t *testing.T) {
		getDevicesTestBadRequests("minVramMb=5&maxVramMb=3")
	})

	t.Run("get devices -- invalid minCudaCores value", func(t *testing.T) {
		getDevicesTestBadRequests("minCudaCores=-5")
	})

	t.Run("get devices -- invalid maxCudaCores value", func(t *testing.T) {
		getDevicesTestBadRequests("maxCudaCores=-5")
	})

	t.Run("get devices -- minCudaCores > maxCudaCores", func(t *testing.T) {
		getDevicesTestBadRequests("minCudaCores=5&maxCudaCores=3")
	})

	t.Run("get devices -- invalid minPricePerHourUsdCents", func(t *testing.T) {
		getDevicesTestBadRequests("minPricePerHourUsdCents=-5")
	})

	t.Run("get devices -- invalid maxPricePerHourUsdCents", func(t *testing.T) {
		getDevicesTestBadRequests("maxPricePerHourUsdCents=-5")
	})

	t.Run("get devices -- minPricePerHourUsdCents > maxPricePerHourUsdCents", func(t *testing.T) {
		getDevicesTestBadRequests("minPricePerHourUsdCents=699&maxPricePerHourUsdCents=399")
	})

	t.Run("get devices -- minDriverVersion > maxDriverVersion", func(t *testing.T) {
		getDevicesTestBadRequests("minDriverVersion=585.10&maxDriverVersion=580.92")
		getDevicesTestBadRequests("minDriverVersion=585.94&maxDriverVersion=585.92")
	})

	getDevicesTestNotFound := func(params string) {
		resetDbContent()
		resp, err := http.Get(baseUrl + "/api/devices?" + params)
		require.NoError(t, err)
		require.NotNil(t, resp)
		defer resp.Body.Close()

		require.Equal(t, http.StatusNotFound, resp.StatusCode)
	}

	t.Run("get devices by name -- not found", func(t *testing.T) {
		getDevicesTestNotFound("name=NONEXISTENT")
	})

	t.Run("get devices by gpu model -- not found", func(t *testing.T) {
		getDevicesTestNotFound("gpuModel=NONEXISTENT")
	})

	t.Run("get devices by minVram -- not found", func(t *testing.T) {
		getDevicesTestNotFound("minVramMb=50000")
	})

	t.Run("get devices by maxVram -- not found", func(t *testing.T) {
		getDevicesTestNotFound("maxVramMb=50")
	})

	t.Run("get devices by minCudaCores -- not found", func(t *testing.T) {
		getDevicesTestNotFound("minCudaCores=50000")
	})

	t.Run("get devices by maxCudaCores -- not found", func(t *testing.T) {
		getDevicesTestNotFound("maxCudaCores=50")
	})

	t.Run("get devices by minPricePerHour -- not found", func(t *testing.T) {
		getDevicesTestNotFound("minPricePerHourUsdCents=3000")
	})

	t.Run("get devices by maxPricePerHour -- not found", func(t *testing.T) {
		getDevicesTestNotFound("maxPricePerHourUsdCents=50")
	})

	t.Run("get devices by minDriverVersion -- not found", func(t *testing.T) {
		getDevicesTestNotFound("minDriverVersion=800.88")
	})

	t.Run("get devices by maxDriverVersion -- not found", func(t *testing.T) {
		getDevicesTestNotFound("maxDriverVersion=1.1")
	})

	t.Run("get devices by states -- not found", func(t *testing.T) {
		getDevicesTestNotFound("states=RENTED&states=REPORTED")
	})
}

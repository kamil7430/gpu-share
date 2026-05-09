package api_tests

import (
	"io"
	"net/http"
	"strings"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/ogen-go/ogen/json"
	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func testOrderDevice(t *testing.T, db *gorm.DB, baseUrl string) {
	testUserPassword, err := auth.HashPassword("TestPassword")
	require.NoError(t, err)

	resetDbContent := func() {
		truncateTables(db)
		db.Exec("INSERT INTO users(id, name, password, admin) VALUES (100, 'TestOwner', ?, 'false');", testUserPassword)
		db.Exec("INSERT INTO users(id, name, password, admin) VALUES (101, 'TestRentingUser', ?, 'false');", testUserPassword)
		db.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd_cents, driver_version_major, driver_version_minor, state, user_id) " +
			"VALUES ('1', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE', '100');")
	}

	resetDbContent()

	loginResp, err := http.Post(baseUrl+"/api/users/login", "application/json", strings.NewReader(`{
		"username": "TestRentingUser",
		"password": "TestPassword"
	}`))
	require.NoError(t, err)
	require.Equal(t, http.StatusOK, loginResp.StatusCode)
	defer loginResp.Body.Close()

	body, err := io.ReadAll(loginResp.Body)
	require.NoError(t, err)

	var tokenObj tokenResponse
	err = json.Unmarshal(body, &tokenObj)
	require.NoError(t, err)

	token := tokenObj.Token

	sendRequest := func(payload string, bearerToken string) *http.Response {
		resetDbContent()
		payloadReader := strings.NewReader(payload)
		req, err := http.NewRequestWithContext(t.Context(), "POST", baseUrl+"/api/orders", payloadReader)
		require.NoError(t, err)

		req.Header.Set("Content-Type", "application/json")
		req.Header.Set("Authorization", "Bearer "+bearerToken)
		resp, err := http.DefaultClient.Do(req)
		require.NoError(t, err)
		return resp
	}

	t.Run("order device -- try to rent own device", func(t *testing.T) {
		resetDbContent()

		loginResp, err := http.Post(baseUrl+"/api/users/login", "application/json", strings.NewReader(`{
			"username": "TestOwner",
			"password": "TestPassword"
		}`))
		require.NoError(t, err)
		require.Equal(t, http.StatusOK, loginResp.StatusCode)
		defer loginResp.Body.Close()

		body, err := io.ReadAll(loginResp.Body)
		require.NoError(t, err)

		var tokenObj tokenResponse
		err = json.Unmarshal(body, &tokenObj)
		require.NoError(t, err)

		token := tokenObj.Token

		payload := `{
			"deviceId": "1",
			"dockerImage": "pytorch/pytorch:2.0-cuda11.7",
			"durationHours": 2
		}`

		response := sendRequest(payload, token)
		defer response.Body.Close()

		require.Equal(t, http.StatusBadRequest, response.StatusCode)
	})

	t.Run("order device -- device rent", func(t *testing.T) {
		resetDbContent()

		payload := `{
			"deviceId": "1",
			"dockerImage": "pytorch/pytorch:2.0-cuda11.7",
			"durationHours": 2
		}`

		response := sendRequest(payload, token)
		defer response.Body.Close()

		body, err := io.ReadAll(response.Body)
		require.NoError(t, err)

		expected := `{
			"orderId": "0",
			"status": "WAITING_FOR_START",
			"connectionDetails": {
				"host": "node-01.gpushare.net",
				"port": 443,
				"protocol": "wss"
			},
			"totalReservedCostCents": 3198
		}`

		require.JSONEq(t, expected, string(body))
	})
}

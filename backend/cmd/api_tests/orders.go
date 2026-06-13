package api_tests

import (
	"io"
	"net/http"
	"strings"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/ogen-go/ogen/json"
	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func testOrderDevice(t *testing.T, db *gorm.DB, baseUrl string) {
	testUserPassword, err := auth.HashPassword("TestPassword")
	require.NoError(t, err)

	resetDbContent := func() {
		truncateTables(db)
		db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (100, 'TestOwner', ?, 'false', 0);", testUserPassword)
		db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (101, 'TestRentingUser1', ?, 'false', 10000);", testUserPassword)
		db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (102, 'TestRentingUser2', ?, 'false', 10);", testUserPassword)
		db.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd_cents, driver_version_major, driver_version_minor, state, user_id) " +
			"VALUES ('1', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE', '100');")
	}

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

	t.Run("order device -- insufficient balance", func(t *testing.T) {
		resetDbContent()

		loginResp, err := http.Post(baseUrl+"/api/users/login", "application/json", strings.NewReader(`{
			"username": "TestRentingUser2",
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

		require.Equal(t, http.StatusPaymentRequired, response.StatusCode)
	})

	t.Run("order device -- device rent", func(t *testing.T) {
		resetDbContent()

		loginResp, err := http.Post(baseUrl+"/api/users/login", "application/json", strings.NewReader(`{
			"username": "TestRentingUser1",
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

		type responseStruct struct {
			OrderId                string
			ConnectionDetails      model.ConnectionDetails
			TotalReservedCostCents int
		}

		response := sendRequest(payload, token)
		defer response.Body.Close()

		body, err = io.ReadAll(response.Body)
		require.NoError(t, err)

		var respStruct responseStruct
		err = json.Unmarshal(body, &respStruct)
		require.NoError(t, err)

		var expectedStruct responseStruct
		err = json.Unmarshal([]byte(`{
			"OrderId": "0",
			"status": "WAITING_FOR_START",
			"connectionDetails": {
				"host": "node-01.gpushare.net",
				"port": "443",
				"protocol": "wss"
			},
			"totalReservedCostCents": 3198
		}`), &expectedStruct)
		require.NoError(t, err)

		require.Equal(t, expectedStruct.ConnectionDetails, respStruct.ConnectionDetails)
		require.Equal(t, expectedStruct.TotalReservedCostCents, respStruct.TotalReservedCostCents)
	})
}

func testGetOrders(t *testing.T, db *gorm.DB, baseUrl string) {
	testUserPassword, err := auth.HashPassword("TestPassword")
	require.NoError(t, err)

	truncateTables(db)
	db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (100, 'TestOwner', ?, 'false', 0);", testUserPassword)
	db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (101, 'TestRentingUser1', ?, 'false', 10000);", testUserPassword)
	db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (102, 'TestRentingUser2', ?, 'false', 10);", testUserPassword)
	db.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd_cents, driver_version_major, driver_version_minor, state, user_id) " +
		"VALUES ('1', 'TestCard1', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE', '100'), " +
		"('2', 'TestCard2', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE', '100'), " +
		"('3', 'TestCard3', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE', '100'), " +
		"('4', 'TestCard4', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE', '100'), " +
		"('5', 'TestCard5', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE', '100');")

	sendOrderRequest := func(payload string, bearerToken string) *http.Response {
		payloadReader := strings.NewReader(payload)
		req, err := http.NewRequestWithContext(t.Context(), "POST", baseUrl+"/api/orders", payloadReader)
		require.NoError(t, err)

		req.Header.Set("Content-Type", "application/json")
		req.Header.Set("Authorization", "Bearer "+bearerToken)
		resp, err := http.DefaultClient.Do(req)
		require.NoError(t, err)
		return resp
	}

	sendRequest := func(orderId string, bearerToken string) *http.Response {
		address := baseUrl + "/api/orders"
		if len(orderId) > 0 {
			address = address + "/" + orderId
		}
		req, err := http.NewRequestWithContext(t.Context(), "GET", address, nil)
		require.NoError(t, err)

		req.Header.Set("Content-Type", "application/json")
		req.Header.Set("Authorization", "Bearer "+bearerToken)
		resp, err := http.DefaultClient.Do(req)
		require.NoError(t, err)
		return resp
	}

	loginResp, err := http.Post(baseUrl+"/api/users/login", "application/json", strings.NewReader(`{
		"username": "TestRentingUser1",
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

	renterToken := tokenObj.Token

	loginResp, err = http.Post(baseUrl+"/api/users/login", "application/json", strings.NewReader(`{
		"username": "TestRentingUser2",
		"password": "TestPassword"
	}`))
	require.NoError(t, err)
	require.Equal(t, http.StatusOK, loginResp.StatusCode)
	defer loginResp.Body.Close()

	body, err = io.ReadAll(loginResp.Body)
	require.NoError(t, err)

	err = json.Unmarshal(body, &tokenObj)
	require.NoError(t, err)

	otherGuyToken := tokenObj.Token

	type orderResponseStruct struct {
		OrderId                string
		ConnectionDetails      model.ConnectionDetails
		TotalReservedCostCents int
	}
	var orderResponse orderResponseStruct

	orderResp := sendOrderRequest(`{
		"deviceId": "1",
		"dockerImage": "pytorch/pytorch:2.0-cuda11.7",
		"durationHours": 2
	}`, renterToken)
	require.Equal(t, http.StatusCreated, orderResp.StatusCode)
	body, err = io.ReadAll(orderResp.Body)
	require.NoError(t, err)
	err = json.Unmarshal(body, &orderResponse)
	require.NoError(t, err)
	firstOrderId := orderResponse.OrderId
	orderResp.Body.Close()

	orderResp = sendOrderRequest(`{
		"deviceId": "2",
		"dockerImage": "pytorch/pytorch:2.0-cuda11.7",
		"durationHours": 2
	}`, renterToken)
	require.Equal(t, http.StatusCreated, orderResp.StatusCode)
	body, err = io.ReadAll(orderResp.Body)
	require.NoError(t, err)
	err = json.Unmarshal(body, &orderResponse)
	require.NoError(t, err)
	secondOrderId := orderResponse.OrderId
	orderResp.Body.Close()

	orderResp = sendOrderRequest(`{
		"deviceId": "3",
		"dockerImage": "pytorch/pytorch:2.0-cuda11.7",
		"durationHours": 2
	}`, renterToken)
	require.Equal(t, http.StatusCreated, orderResp.StatusCode)
	body, err = io.ReadAll(orderResp.Body)
	require.NoError(t, err)
	err = json.Unmarshal(body, &orderResponse)
	require.NoError(t, err)
	thirdOrderId := orderResponse.OrderId
	orderResp.Body.Close()

	t.Run("get orders -- not logged in", func(t *testing.T) {
		resp, err := http.Get(baseUrl + "/api/orders")
		require.NoError(t, err)
		require.Equal(t, http.StatusUnauthorized, resp.StatusCode)
		resp.Body.Close()
	})

	t.Run("get orders -- empty order list", func(t *testing.T) {
		resp := sendRequest("", otherGuyToken)
		require.Equal(t, http.StatusOK, resp.StatusCode)
		defer resp.Body.Close()
		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)
		require.JSONEq(t, "[]", string(body))
	})

	t.Run("get orders -- some orders on list", func(t *testing.T) {
		resp := sendRequest("", renterToken)
		require.Equal(t, http.StatusOK, resp.StatusCode)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		var response []orderResponseStruct
		err = json.Unmarshal(body, &response)
		require.NoError(t, err)

		require.Equal(t, 3, len(response))

		ids := []string{firstOrderId, secondOrderId, thirdOrderId}
		for _, elem := range response {
			ids, err = remove(ids, elem.OrderId)
			require.NoError(t, err)
		}
	})

	t.Run("get order by id -- not logged in", func(t *testing.T) {
		resp, err := http.Get(baseUrl + "/api/orders/" + firstOrderId)
		require.NoError(t, err)
		require.Equal(t, http.StatusUnauthorized, resp.StatusCode)
		resp.Body.Close()
	})

	t.Run("get order by id -- not user's order", func(t *testing.T) {
		resp := sendRequest(firstOrderId, otherGuyToken)
		require.Equal(t, http.StatusUnauthorized, resp.StatusCode)
		resp.Body.Close()
	})

	t.Run("get order by id -- invalid order id", func(t *testing.T) {
		resp := sendRequest("696969", renterToken)
		require.Equal(t, http.StatusNotFound, resp.StatusCode)
		resp.Body.Close()
	})

	t.Run("get order by id -- correct case", func(t *testing.T) {
		resp := sendRequest(firstOrderId, renterToken)
		require.Equal(t, http.StatusOK, resp.StatusCode)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		var response orderResponseStruct
		err = json.Unmarshal(body, &response)
		require.NoError(t, err)

		require.Equal(t, firstOrderId, response.OrderId)
	})
}

package main

import (
	"errors"
	"io"
	"log"
	"net/http"
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/backend/cmd/server"
	"github.com/kamil7430/gpu-share/backend/internal"
	"github.com/stretchr/testify/require"
)

const baseUrl string = "http://localhost:2137"

func TestApi(t *testing.T) {
	db, err := internal.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	srv := server.NewServer(tx)
	defer srv.Shutdown(t.Context())
	go func() {
		err := srv.ListenAndServe()
		if !errors.Is(err, http.ErrServerClosed) {
			log.Fatal(err)
		}
	}()

	log.Println("Checking whether server is up...")
	retries := 10
	i := 0
	for ; i < retries; i += 1 {
		resp, err := http.Get(baseUrl + "/health")
		if err == nil && resp.StatusCode == 200 {
			break
		}
		log.Printf("Failed, retrying in one second... (try no.: %v/%v)\n", i+1, retries)
		time.Sleep(time.Second)
	}
	require.Less(t, i, retries)

	deviceId := "123"

	tx.Exec("TRUNCATE TABLE devices;")
	tx.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd, driver_version, state) " +
		"VALUES ('" + deviceId + "', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '15.99', '595.97', 'AVAILABLE');")

	t.Run("device status by id", func(t *testing.T) {
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
	/*
		t.Run("device register", func(t *testing.T) {
			payload := `{
		        "name": "Moja karta RTX 4090",
		        "gpu_model": "NVIDIA GeForce RTX 4090",
		        "vram_mb": 24576,
		        "cuda_cores": 16384,
		        "price_per_hour_usd": 0.45,
		        "driver_version": "535.104",
		        "supported_frameworks": ["pytorch", "tensorflow"]
		    }`

			response, err := http.Post(baseUrl + "/api/devices", "application/json", strings.NewReader(payload))
			require.NoError(t, err)
			defer response.Body.Close()

			body, err := io.ReadAll(response.Body)
			require.NoError(t, err)

			expected := `{
				"device_id": "550e8400-e29b-41d4-a716-446655440000",
				"owner_id": "user_12345",
				"status": "AVAILABLE",
				"created_at": "2026-01-06T12:34:56Z"
			}`

			require.JSONEq(t, expected, string(body))
		})

		t.Run("device rent", func(t *testing.T) {
			payload := `{
				"device_id": "550e8400-e29b-41d4-a716-446655440000",
				"docker_image": "pytorch/pytorch:2.0-cuda11.7",
				"duration_hours": 2
			}`

			response, err := http.Post(baseUrl + "/api/orders", "application/json", strings.NewReader(payload))
			require.NoError(t, err)
			defer response.Body.Close()

			body, err := io.ReadAll(response.Body)
			require.NoError(t, err)

			expected := `{
				"order_id": "ord_123456789",
				"status": "WAITING_FOR_START",
				"connection_details": {
					"host": "node-01.gpushare.net",
					"port": 443,
					"protocol": "wss"
				},
				"total_reserved_cost": 0.90
			}`

			require.JSONEq(t, expected, string(body))
		}) */
}

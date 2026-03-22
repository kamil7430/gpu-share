package main

import (
	"encoding/json"
	"io"
	"net/http"
	"strings"
	"testing"

	"github.com/stretchr/testify/require"
)

func TestDeviceRegister(t *testing.T) {
	payload := `{
					"name": "Moja karta RTX 4090",
					"gpu_model": "NVIDIA GeForce RTX 4090",
					"vram_mb": 24576,
					"cuda_cores": 16384,
					"price_per_hour_usd": 0.45,
					"driver_version": "535.104",
					"supported_frameworks": ["pytorch", "tensorflow"]
				}`

	response, err := http.Post("https://localhost:8080/api/devices", "application/json", strings.NewReader(payload))
	require.NoError(t, err)
	defer response.Body.Close()

	expected, _ := json.Marshal(`{
									"device_id": "550e8400-e29b-41d4-a716-446655440000",
									"owner_id": "user_12345",
									"status": "AVAILABLE",
									"created_at": "2026-01-06T12:34:56Z"
								 }`)
	bytes, err := io.ReadAll(response.Body)
	require.NoError(t, err)
	actual, err := json.Marshal(bytes)
	require.NoError(t, err)

	require.JSONEq(t, string(expected), string(actual))
}

func TestDeviceStatus(t *testing.T) {
	deviceID := "123"

	resp, err := http.Get("https://localhost:8080/api/devices/" + deviceID + "/status")
	require.NoError(t, err)
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	require.NoError(t, err)

	expected := `{
		"deviceId": "123",
		"state": "AVAILABLE",
		"temperatureC": 68,
		"utilizationPercent": 72,
		"memoryUsedMb": 10240,
		"lastHeartbeat": "2026-01-06T12:34:56Z"
	}`

	require.JSONEq(t, expected, string(body))
}

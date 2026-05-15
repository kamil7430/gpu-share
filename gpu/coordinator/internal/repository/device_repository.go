package repository

import (
	"encoding/json"
	"io"
	"log"
	"net/http"
	"os"
)

type Device struct {
	DeviceID             string `json:"deviceId"`
	Name                 string `json:"name"`
	GPUModel             string `json:"gpuModel"`
	VramMb               int    `json:"vramMb"`
	CudaCores            int    `json:"cudaCores"`
	PricePerHourUsdCents int    `json:"pricePerHourUsdCents"`
	DriverVersion        string `json:"driverVersion"`
	State                string `json:"state"`
}

type errorResponse struct {
	ErrorMessage string `json:"errorMessage"`
}

type DeviceRepository struct{}

func (dr *DeviceRepository) GetDevices(token string) ([]Device, error) {
	backIp := os.Getenv("BACKEND_IP")
	if backIp == "" {
		backIp = "10.5.0.2"
	}

	url := "http://" + backIp + ":2137/api/devices"

	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		log.Fatal(err)
	}

	req.Header.Set("Authorization", "Bearer "+token)

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		log.Fatalf("couldn't connect to backend (%v)", err)
	}
	defer resp.Body.Close()

	var devices []Device
	switch resp.StatusCode {
	case http.StatusOK:
		if err := json.NewDecoder(resp.Body).Decode(&devices); err != nil {
			log.Fatal(err)
		}

	case http.StatusBadRequest:
		var errResp errorResponse

		if err := json.NewDecoder(resp.Body).Decode(&errResp); err != nil {
			log.Fatal(err)
		}

		log.Fatalf("bad request: %s", errResp.ErrorMessage)

	case http.StatusUnauthorized:
		log.Fatal("invalid or expired token")

	case http.StatusNotFound:
		log.Println("not found")

	default:
		body, _ := io.ReadAll(resp.Body)
		log.Fatalf("backend returned %s: %s", resp.Status, string(body))
	}

	return devices, nil
}

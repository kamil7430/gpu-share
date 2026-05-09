package agent

import (
	"encoding/json"
	"fmt"
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

func ListDevices() {
	token, err := LoadToken()
	if err != nil {
		log.Fatal("not logged in")
	}

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

	switch resp.StatusCode {
	case http.StatusOK:
		var devices []Device

		if err := json.NewDecoder(resp.Body).Decode(&devices); err != nil {
			log.Fatal(err)
		}

		if len(devices) == 0 {
			fmt.Println("no registered devices")
			return
		}

		fmt.Println("your devices:")

		for i, d := range devices {
			fmt.Printf(
				"[%d] %s | %s | %d MB VRAM | %d CUDA cores | $%.2f/h | %s\n",
				i,
				d.Name,
				d.GPUModel,
				d.VramMb,
				d.CudaCores,
				float64(d.PricePerHourUsdCents)/100.0,
				d.State,
			)
		}

	case http.StatusNotFound:
		fmt.Println("no registered devices")

	case http.StatusBadRequest:
		var errResp errorResponse

		if err := json.NewDecoder(resp.Body).Decode(&errResp); err != nil {
			log.Fatal(err)
		}

		log.Fatalf("bad request: %s", errResp.ErrorMessage)

	case http.StatusUnauthorized:
		log.Fatal("invalid or expired token")

	default:
		body, _ := io.ReadAll(resp.Body)
		log.Fatalf("backend returned %s: %s", resp.Status, string(body))
	}
}

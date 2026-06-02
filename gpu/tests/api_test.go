package tests

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/gpu/agent/agent"
	"github.com/kamil7430/gpu-share/gpu/agent/cli"
	"github.com/kamil7430/gpu-share/gpu/coordinator/cmd/server"
	"github.com/stretchr/testify/require"
)

var ip = func() string {
	ip := os.Getenv("BACKEND_IP")
	if ip == "" {
		log.Fatal("invalid value of `BACKEND_IP` env variable")
	}
	return ip
}()

const backPort = "2137"
const restPort = "2138"
const grpcPort = "2139"

var restUrl string = fmt.Sprintf("http://%v:%v", ip, restPort)
var grpcUrl string = fmt.Sprintf("%v:%v", ip, grpcPort)

func newRequest(t *testing.T, method string, url string, body []byte) *http.Request {
	req, err := http.NewRequestWithContext(
		t.Context(),
		method,
		restUrl+url,
		bytes.NewBuffer(body),
	)
	require.NoError(t, err)
	req.Header.Set("Authorization", "test")
	req.Header.Set("Content-Type", "application/json")

	return req
}

func registerUser(t *testing.T) {
	body, _ := json.Marshal(map[string]string{
		"username": "test",
		"password": "maklowicz",
	})

	req, err := http.NewRequestWithContext(
		t.Context(),
		"POST",
		"http://"+ip+":"+backPort+"/api/users/register",
		bytes.NewBuffer(body),
	)
	require.NoError(t, err)
	req.Header.Set("Content-Type", "application/json")
	resp, err := http.DefaultClient.Do(req)
	require.NoError(t, err)
	require.Equal(t, http.StatusCreated, resp.StatusCode)
}

func registerDevice(t *testing.T, token string) string {
	body, _ := json.Marshal(map[string]any{
		"name":                 "GPU_Maklowicza1",
		"gpuModel":             "RTX 5090",
		"vramMb":               32000,
		"cudaCores":            21760,
		"pricePerHourUsdCents": 2000,
		"driverVersion":        "596.36",
	})

	req, err := http.NewRequestWithContext(
		t.Context(),
		"POST",
		"http://"+ip+":"+backPort+"/api/devices",
		bytes.NewBuffer(body),
	)
	require.NoError(t, err)

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", "Bearer "+token)

	resp, err := http.DefaultClient.Do(req)
	require.NoError(t, err)
	defer resp.Body.Close()

	require.Equal(t, http.StatusCreated, resp.StatusCode)

	var result struct {
		DeviceID      string `json:"deviceId"`
		OwnerUsername string `json:"ownerUsername"`
		State         string `json:"state"`
		CreatedAt     string `json:"createdAt"`
	}

	err = json.NewDecoder(resp.Body).Decode(&result)
	require.NoError(t, err)

	require.NotEmpty(t, result.DeviceID)

	return result.DeviceID
}

func login(t *testing.T) string {
	cli.LoginCmd([]string{"-u", "test", "-p", "maklowicz"})
	token, err := cli.LoadToken()
	require.NoError(t, err)
	return token
}

func startAgent(t *testing.T, agentId string, token string) {
	stream, err := agent.StartGrpcClient(t.Context(), grpcUrl)
	require.NoError(t, err)

	err = agent.SendHelloMessage(stream, agentId, token)
	require.NoError(t, err)

	go agent.SendHeartbeats(t.Context(), stream, agentId, token)

	err = agent.ReceiveLoop(t.Context(), stream, agentId)
	require.NoError(t, err)
}

func TestApi(t *testing.T) {
	go server.InitializeSystem(t.Context(), restUrl, grpcUrl)

	log.Println("Checking whether server is up...")
	retries := 10
	i := 0
	req := newRequest(t, http.MethodGet, "/health", nil)
	for ; i < retries; i += 1 {
		resp, err := http.DefaultClient.Do(req)
		if err == nil && resp.StatusCode == 200 {
			break
		}
		log.Printf("Failed, retrying in one second... (try no.: %v/%v)\n", i+1, retries)
		time.Sleep(time.Second)
	}
	require.Less(t, i, retries, "Could not connect to server!")
	log.Println("Server is up!")

	registerUser(t)
	token := login(t)

	agentId := registerDevice(t, token)
	go startAgent(t, agentId, token)

	log.Println("Checking whether agent is connected...")
	i = 0
	req = newRequest(t, http.MethodGet, "/api/agents/"+agentId+"/status", nil)
	for ; i < retries; i += 1 {
		resp, err := http.DefaultClient.Do(req)
		if err == nil && resp.StatusCode == 200 {
			break
		}
		log.Printf("Failed, retrying in one second... (try no.: %v/%v)\n", i+1, retries)
		time.Sleep(time.Second)
	}
	require.Less(t, i, retries, "Agent unreachable!")
	log.Println("Agent connected! Running the tests...")

	t.Run("Agent job", func(t *testing.T) {
		payload := fmt.Sprintf(`{
			"deviceId": "%v",
			"resources": {
				"vRamMb": 200
			}
		}`, agentId)

		req := newRequest(t, http.MethodPost, "/api/jobs", []byte(payload))
		resp, err := http.DefaultClient.Do(req)
		require.NoError(t, err)
		require.Equal(t, http.StatusCreated, resp.StatusCode)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		expected := `{
			"jobId": "task-0"
		}`

		require.JSONEq(t, expected, string(body))
	})
}

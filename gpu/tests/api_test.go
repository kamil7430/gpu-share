package tests

import (
	"fmt"
	"io"
	"log"
	"net/http"
	"strings"
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/gpu/agent/agent"
	"github.com/kamil7430/gpu-share/gpu/coordinator/cmd/server"
	"github.com/stretchr/testify/require"
)

const baseUrl = "localhost"
const restPort = "22138"
const grpcPort = "22139"

var restUrl string = fmt.Sprintf("http://%v:%v", baseUrl, restPort)
var grpcUrl string = fmt.Sprintf("%v:%v", baseUrl, grpcPort)

func startAgent(t *testing.T, agentId string) {
	stream, err := agent.StartGrpcClient(t.Context(), grpcUrl)
	if err != nil {
		log.Fatal(err)
	}

	agent.SendHelloMessage(stream, agentId)
	go agent.SendHeartbeats(stream, agentId)
	agent.ReceiveLoop(stream, agentId)
}

func TestApi(t *testing.T) {
	go server.InitializeSystem(t.Context(), restUrl, grpcUrl)

	log.Println("Checking whether server is up...")
	retries := 10
	i := 0
	for ; i < retries; i += 1 {
		resp, err := http.Get(restUrl + "/health")
		if err == nil && resp.StatusCode == 200 {
			break
		}
		log.Printf("Failed, retrying in one second... (try no.: %v/%v)\n", i+1, retries)
		time.Sleep(time.Second)
	}
	require.Less(t, i, retries, "Could not connect to server!")
	log.Println("Server is up!")

	agentId := "123"
	go startAgent(t, agentId)

	log.Println("Checking whether agent is connected...")
	i = 0
	for ; i < retries; i += 1 {
		resp, err := http.Get(restUrl + "/api/agents/" + agentId + "/status")
		if err == nil && resp.StatusCode == 200 {
			break
		}
		log.Printf("Failed, retrying in one second... (try no.: %v/%v)\n", i+1, retries)
		time.Sleep(time.Second)
	}
	require.Less(t, i, retries, "Agent unreachable!")
	log.Println("Agent connected! Running the tests...")

	t.Run("Agent job", func(t *testing.T) {
		payload := `{
			"deviceId": "123",
			"resources": {
				"vRamMb": 200
			}
		}`
		reader := strings.NewReader(payload)

		resp, err := http.Post(restUrl+"/api/jobs", "application/json", reader)
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

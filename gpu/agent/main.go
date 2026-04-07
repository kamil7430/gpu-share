package main

import (
	"context"
	"fmt"
	"log"
	"math/rand"
	"os"

	"github.com/kamil7430/gpu-share/gpu/agent/agent"
)

func main() {
	ip := os.Getenv("GPU_IP")
	if ip == "" {
		log.Fatal("invalid value of `GPU_IP` env variable")
	}
	stream, err := agent.StartGrpcClient(context.Background(), ip+":2139")
	if err != nil {
		log.Fatal(err)
	}

	// TODO: this should be assigned by the `backend`, but it requires the
	// `POST /api/devices` endpoint to be implemented
	agentId := fmt.Sprintf("%v", rand.Int()%1000)
	if err := agent.SendHelloMessage(stream, agentId); err != nil {
		log.Fatalf("couldn't connect to coordinator (%v)", err)
	}

	go agent.SendHeartbeats(context.Background(), stream, agentId)

	agent.ReceiveLoop(context.Background(), stream, agentId)
}

package main

import (
	"context"
	"fmt"
	"log"
	"math/rand"

	"github.com/kamil7430/gpu-share/gpu/agent/agent"
)

func main() {
	stream, err := agent.StartGrpcClient(context.Background(), "localhost:2139")
	if err != nil {
		log.Fatal(err)
	}

	// TODO: this should be assigned by the `backend`, but it requires the
	// `POST /api/devices` endpoint to be implemented
	agentId := fmt.Sprintf("%v", rand.Int()%1000)
	agent.SendHelloMessage(stream, agentId)

	go agent.SendHeartbeats(stream, agentId)

	agent.ReceiveLoop(stream, agentId)
}


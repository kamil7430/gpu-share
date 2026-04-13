package main

import (
	"context"
	"flag"
	"fmt"
	"log"
	"math/rand"
	"os"

	"github.com/kamil7430/gpu-share/gpu/agent/agent"
)

func main() {
	env_ip := os.Getenv("GPU_IP")
	if env_ip == "" {
		env_ip = "10.5.0.3"
	}
	ip := flag.String("ip", env_ip, "IP of the coordinator service")
	port := flag.String("port", "2139", "port of the coordinator service")
	flag.Parse()

	log.Printf("Connecting to %v%v...", *ip, *port)
	stream, err := agent.StartGrpcClient(context.Background(), *ip+":"+*port)
	if err != nil {
		log.Fatal(err)
	}

	// TODO: this should be assigned by the `backend`, but it requires the
	// `POST /api/devices` endpoint to be implemented
	agentId := fmt.Sprintf("%v", rand.Int()%1000)
	if err := agent.SendHelloMessage(stream, agentId); err != nil {
		log.Fatalf("couldn't connect to coordinator (%v)", err)
	}

	log.Printf("Agent %v connected to coordinator at %v", agentId, *ip)

	go agent.SendHeartbeats(context.Background(), stream, agentId)

	agent.ReceiveLoop(context.Background(), stream, agentId)
}

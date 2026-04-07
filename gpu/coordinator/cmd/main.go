package main

import (
	"context"
	"log"
	"os"

	"github.com/kamil7430/gpu-share/gpu/coordinator/cmd/server"
)

func main() {
	ip := os.Getenv("GPU_IP")
	if ip == "" {
		log.Fatal("invalid value of `GPU_IP` env variable")
	}

	server.InitializeSystem(context.Background(), ip+":2138", ip+":2139")
}

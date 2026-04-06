package main

import (
	"context"

	"github.com/kamil7430/gpu-share/gpu/coordinator/cmd/server"
)

func main() {
	server.InitializeSystem(context.Background(), "http://localhost:2138", ":2139")
}

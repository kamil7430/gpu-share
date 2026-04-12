package main

import (
	"context"
	"flag"
	"os"

	"github.com/kamil7430/gpu-share/gpu/coordinator/cmd/server"
)

func main() {
	env_ip := os.Getenv("GPU_IP")
	if env_ip == "" {
		env_ip = "10.5.0.3"
	}
	ip := flag.String("ip", env_ip, "IP of the coordinator service")
	rest_port := flag.String("rest-port", "2138", "port of the coordinator REST service")
	grpc_port := flag.String("grpc-port", "2139", "port of the coordinator gRPC service")
	flag.Parse()

	server.InitializeSystem(context.Background(), *ip+":"+*rest_port, *ip+":"+*grpc_port)
}

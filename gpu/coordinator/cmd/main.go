package main

import (
	"context"
	"flag"
	"os"

	"github.com/kamil7430/gpu-share/gpu/coordinator/cmd/server"
)

func main() {
	envIp := os.Getenv("GPU_IP")
	if envIp == "" {
		envIp = "10.5.0.3"
	}
	ip := flag.String("ip", envIp, "IP of the coordinator service")
	restPort := flag.String("rest-port", "2138", "port of the coordinator REST service")
	grpcPort := flag.String("grpc-port", "2139", "port of the coordinator gRPC service")
	flag.Parse()

	server.InitializeSystem(context.Background(), *ip+":"+*restPort, *ip+":"+*grpcPort)
}

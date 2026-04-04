package main

import (
	"log"
	"net"
	"net/http"

	"github.com/kamil7430/gpu-share/gpu/coordinator/api"
	"github.com/kamil7430/gpu-share/gpu/coordinator/handler"
	"github.com/kamil7430/gpu-share/gpu/coordinator/repository"
	"github.com/kamil7430/gpu-share/gpu/coordinator/service"
	"github.com/kamil7430/gpu-share/gpu/proto"
	"google.golang.org/grpc"
)

func startRestServer(port string, as *service.AgentService) {
	srv, err := api.NewServer(handler.NewRestHandler(service.NewJobsService(as)))
	if err != nil {
		log.Fatal(err)
	}

	s := http.Server{Addr: port, Handler: srv}

	go func() {
		log.Printf("Coordinator REST listening on %v\n", port)
		if err := s.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Fatal(err)
		}
	}()
}

func main() {
	as := service.NewAgentService(repository.NewAgentRepository())
	startRestServer(":2138", as)

	port := ":2139"
	lis, err := net.Listen("tcp", port)
	if err != nil {
		log.Fatal(err)
	}

	grpcServer := grpc.NewServer()
	proto.RegisterAgentServiceServer(grpcServer, as)

	log.Printf("Coordinator listening on %v\n", port)
	if err := grpcServer.Serve(lis); err != nil {
		log.Fatal(err)
	}
}

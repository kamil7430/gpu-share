package server

import (
	"context"
	"log"
	"net"
	"net/http"

	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/api"
	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/handler"
	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/repository"
	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/service"
	"github.com/kamil7430/gpu-share/gpu/proto"
	"google.golang.org/grpc"
)

func InitializeSystem(ctx context.Context, restUrl string, grpcUrl string) {
	as := service.NewAgentService(repository.NewAgentRepository())
	StartRestServer(restUrl, as)
	StartGrpcServer(ctx, grpcUrl, as)
}

func StartRestServer(addr string, as *service.AgentService) {
	srv, err := api.NewServer(handler.NewRestHandler(as))
	if err != nil {
		log.Fatal(err)
	}

	s := http.Server{
		Addr:    addr[7:], // "http://"
		Handler: srv,
	}

	go func() {
		log.Printf("Coordinator REST listening on %v\n", addr)
		if err := s.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Fatal(err)
		}
	}()
}

func StartGrpcServer(ctx context.Context, addr string, as *service.AgentService) {
	lc := net.ListenConfig{}
	lis, err := lc.Listen(ctx, "tcp", addr)
	if err != nil {
		log.Fatal(err)
	}

	grpcServer := grpc.NewServer()
	proto.RegisterAgentServiceServer(grpcServer, as)

	log.Printf("Coordinator listening on %v\n", addr)
	if err := grpcServer.Serve(lis); err != nil {
		log.Fatal(err)
	}
}

package server

import (
	"context"
	"log"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/service"
	"gorm.io/gorm"
)

type Sauron struct {
	service.DeviceService
}

func NewServer(db *gorm.DB) *api.Server {
	deviceRepo := repository.NewDatabaseDeviceRepository(db, context.Background())
	gpuRepo := repository.NewMockGpuRepository()

	deviceService := service.NewDeviceService(deviceRepo, gpuRepo)

	sauron := Sauron{
		deviceService,
	}

	server, err := api.NewServer(&sauron)
	if err != nil {
		log.Fatal(err)
	}

	return server

	//deviceHandler := api.NewStrictHandler(&deviceService, nil)
	//api.HandlerFromMux(deviceHandler, mux)
	//
	//mux.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
	//	w.WriteHeader(http.StatusOK)
	//	w.Write([]byte("OK"))
	//})
	//
	//loginHandler := handler.NewLoginHandler(&service.UserService{})
	//
	//mux.HandleFunc("POST /login", loginHandler.Login)
	//
	//return &http.Server{Addr: ":2137", Handler: mux}
}

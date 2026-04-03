package server

import (
	"log"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/service"
	"gorm.io/gorm"
)

func NewServer(db *gorm.DB) *http.Server {
	deviceRepo := repository.NewDatabaseDeviceRepository(db)
	gpuRepo := repository.NewMockGpuRepository()

	sauron := Sauron{
		service.NewHealthService(),
		service.NewDeviceService(deviceRepo, gpuRepo),
	}

	srv, err := api.NewServer(&struct{
		service.DeviceService
		service.HealthService
	}{
		service.NewDeviceService(),
		service.NewHealthService()
	})
	if err != nil {
		log.Fatal(err)
	}

	return &http.Server{Addr: ":2137", Handler: srv}

	//loginHandler := handler.NewLoginHandler(&service.UserService{})
	//
	//mux.HandleFunc("POST /login", loginHandler.Login)
	//
	//return &http.Server{Addr: ":2137", Handler: mux}
}

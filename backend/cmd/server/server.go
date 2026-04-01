package server

import (
	"context"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/handler"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/service"
	"gorm.io/gorm"
)

func NewServer(db *gorm.DB) *http.Server {
	mux := http.NewServeMux()

	deviceRepo := repository.NewDatabaseDeviceRepository(db, context.Background())
	gpuRepo := repository.NewMockGpuRepository()

	deviceService := service.NewDeviceService(deviceRepo, gpuRepo)

	// deviceHandler := handler.NewDeviceHandler(&deviceService)
	// deviceHandler := api.HandlerFromMux()
	deviceHandler := api.NewStrictHandler(&deviceService, nil)
	api.HandlerFromMux(deviceHandler, mux)

	mux.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		w.Write([]byte("OK"))
	})
	// mux.HandleFunc("GET /api/devices/{id}/status", deviceHandler.HandleDeviceStatusId)
	//
	// mux.HandleFunc("GET /api/devices/{id}", deviceService.GetDevice)

	loginHandler := handler.NewLoginHandler(&service.UserService{})

	mux.HandleFunc("POST /login", loginHandler.Login)

	return &http.Server{Addr: ":2137", Handler: mux}
}

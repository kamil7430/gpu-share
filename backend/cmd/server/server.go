package main

import (
	"context"
	"net/http"

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

	deviceHandler := handler.NewDeviceHandler(&deviceService)

	mux.HandleFunc("GET /api/devices/{id}/status", deviceHandler.HandleDeviceStatusId)

	loginHandler := handler.NewLoginHandler(&service.UserService{})

	mux.HandleFunc("POST /login", loginHandler.Login)

	return &http.Server{Addr: ":2137", Handler: mux}
}

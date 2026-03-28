package main

import (
	"context"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/internal/handler"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/service"
	"gorm.io/gorm"
)

func NewServer(db *gorm.DB) http.Handler {
	mux := http.NewServeMux()

	deviceRepo := repository.NewDatabaseDeviceRepository(db, context.Background())
	gpuRepo := repository.NewMockGpuRepository()

	deviceService := service.NewDeviceService(deviceRepo, gpuRepo)

	deviceHandler := handler.NewDeviceHandler(&deviceService)

	//mux.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
	//	if err := db.Ping(); err != nil {
	//		http.Error(w, "DB not reachable", 500)
	//		return
	//	}
	//	w.Write([]byte("OK"))
	//})

	mux.HandleFunc("GET /api/devices/{id}", deviceHandler.HandleDeviceId)

	//fs := http.FileServer(http.Dir("/app/frontend"))
	//mux.Handle("/", fs)

	//mux.HandleFunc("GET /", func(w http.ResponseWriter, req *http.Request) {
	//	w.Write([]byte("Halo Welt!"))
	//})

	return mux
}

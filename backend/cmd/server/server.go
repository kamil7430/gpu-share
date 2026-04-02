package server

import (
	"context"
	"log"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/service"
	"gorm.io/gorm"
)
// Jeden, by wszystkie zgromadzić i w ciemności związać
// W Krainie Mordor, gdzie zaległy cienie.
type Sauron struct {
	service.HealthService
	service.DeviceService
}

func NewServer(db *gorm.DB) *http.Server {
	deviceRepo := repository.NewDatabaseDeviceRepository(db, context.Background())
	gpuRepo := repository.NewMockGpuRepository()

	sauron := Sauron{
		service.NewHealthService(),
		service.NewDeviceService(deviceRepo, gpuRepo),
	}

	apiServer, err := api.NewServer(&sauron)
	if err != nil {
		log.Fatal(err)
	}

	return &http.Server{Addr: ":2137", Handler: apiServer}

	//loginHandler := handler.NewLoginHandler(&service.UserService{})
	//
	//mux.HandleFunc("POST /login", loginHandler.Login)
	//
	//return &http.Server{Addr: ":2137", Handler: mux}
}

package server

import (
	"log"
	"net/http"
	"os"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/service"
	"gorm.io/gorm"
)

// Jeden, by wszystkie zgromadzić i w ciemności związać
// W Krainie Mordor, gdzie zaległy cienie.
type sauron struct {
	service.HealthService
	service.DeviceService
}

func NewServer(db *gorm.DB, repos *Repos) *http.Server {
	ourSauron := sauron{
		service.NewHealthService(),
		service.NewDeviceService(repos.DeviceRepo, repos.GpuRepo),
	}
	srv, err := api.NewServer(&ourSauron)
	if err != nil {
		log.Fatal(err)
	}

	ip := os.Getenv("BACKEND_IP")
	if ip == "" {
		log.Fatal("invalid value of `BACKEND_IP` env variable")
	}
	return &http.Server{Addr: ip + ":2137", Handler: srv}
}

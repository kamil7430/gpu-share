package server

import (
	"flag"
	"log"
	"net/http"
	"os"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/service"
)

// Jeden, by wszystkie zgromadzić i w ciemności związać
// W Krainie Mordor, gdzie zaległy cienie.
type sauron struct {
	service.HealthService
	service.DeviceService
}

func NewServer(repos *Repos) *http.Server {
	sauron := sauron{
		service.NewHealthService(),
		service.NewDeviceService(repos.DeviceRepo, repos.GpuRepo),
	}
	userService := service.NewUserService(repos.UserRepo)

	srv, err := api.NewServer(&sauron, &userService)
	if err != nil {
		log.Fatal(err)
	}

	envIp := os.Getenv("BACKEND_IP")
	if envIp == "" {
		envIp = "10.5.0.2"
	}
	ip := flag.String("ip", envIp, "IP of the backend service")
	port := flag.String("port", "2137", "port of the backend service")
	flag.Parse()
	return &http.Server{Addr: *ip + ":" + *port, Handler: srv}
}

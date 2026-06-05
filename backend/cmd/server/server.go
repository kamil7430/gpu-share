package server

import (
	"flag"
	"log"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/service"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
)

// Jeden, by wszystkie zgromadzić i w ciemności związać
// W Krainie Mordor, gdzie zaległy cienie.
type sauron struct {
	service.HealthService
	service.DeviceService
	service.UserService
	service.OrderService
}

func NewServer(store repository.Store) *http.Server {
	sauron := sauron{
		service.NewHealthService(),
		service.NewDeviceService(store),
		service.NewUserService(store),
		service.NewOrderService(store),
	}

	srv, err := api.NewServer(&sauron, &sauron.UserService)
	if err != nil {
		log.Fatal(err)
	}

	envIp := utils.GetenvOrDefault("BACKEND_IP", "127.0.0.1")
	ip := flag.String("ip", envIp, "IP of the backend service")
	port := flag.String("port", "2137", "port of the backend service")
	flag.Parse()
	return &http.Server{Addr: *ip + ":" + *port, Handler: srv}
}

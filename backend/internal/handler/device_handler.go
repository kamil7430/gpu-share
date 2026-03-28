package handler

import (
	"log"
	"net/http"
	"strconv"
	"strings"

	"github.com/kamil7430/gpu-share/backend/internal/service"
)

type DeviceHandler struct {
	s *service.DeviceService
}

func NewDeviceHandler(s *service.DeviceService) DeviceHandler {
	return DeviceHandler{s}
}

func (h *DeviceHandler) HandleDeviceStatusId(w http.ResponseWriter, r *http.Request) {
	parts := strings.Split(r.URL.Path, "/")
	log.Println(parts)

	if len(parts) != 5 {
		http.Error(w, "invalid URL", 400)
		return
	}

	id, err := strconv.Atoi(parts[3])
	if err != nil {
		http.Error(w, "invalid device id", 400)
		return
	}

	status, err := h.s.GetDeviceStatusById(id)
	if err != nil {
		http.Error(w, "internal server error", 500)
		log.Println(err)
		return
	}

	writeJson(w, status)
	w.WriteHeader(http.StatusOK)
}

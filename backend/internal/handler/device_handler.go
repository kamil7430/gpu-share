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

func (h *DeviceHandler) HandleDeviceId(w http.ResponseWriter, r *http.Request) {
	parts := strings.Split(r.URL.Path, "/")

	if len(parts) == 4 && parts[3] == "status" {
		id, err := strconv.Atoi(parts[2])
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

		writeJSON(w, status)
		w.WriteHeader(http.StatusOK)
		return
	}

	if len(parts) == 3 {
		panic("not implemented")
		w.WriteHeader(http.StatusOK)
		return
	}

	http.Error(w, "invalid URL", 400)
	return
}

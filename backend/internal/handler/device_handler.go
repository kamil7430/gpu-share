package handler

import (
	"net/http"
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

	if len(parts) != 5 {
		http.Error(w, "invalid URL", 400)
		return
	}

	// status, err := h.s.GetDeviceStatusById(parts[3])
	// if err != nil {
	// 	http.Error(w, "internal server error", 500)
	// 	log.Println(err)
	// 	return
	// }

	// w.WriteHeader(http.StatusOK)
	// writeJson(w, status)
}

package handler

import (
	"context"
	"log"

	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/api"
	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/service"
)

type RestHandler struct {
	*service.AgentService
}

var _ api.Handler = &RestHandler{}

func NewRestHandler(as *service.AgentService) *RestHandler {
	return &RestHandler{as}
}

func (*RestHandler) GetHealth(ctx context.Context) error {
	return nil
}

func (*RestHandler) NewError(ctx context.Context, err error) *api.ErrorStatusCode {
	log.Println(err)
	return &api.ErrorStatusCode{
		StatusCode: 500,
		Response:   api.Error(err.Error()),
	}
}

package handler

import (
	"context"
	"net/http"

	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/api"
	"github.com/kamil7430/gpu-share/gpu/coordinator/internal/service"
	"github.com/ogen-go/ogen/ogenerrors"
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

func (*RestHandler) NewError(ctx context.Context, err error) *api.DefaultStatusCode {
	code := http.StatusInternalServerError
	if e, ok := err.(ogenerrors.Error); ok {
		code = e.Code()
	}

	return &api.DefaultStatusCode{
		StatusCode: code,
		Response:   api.Error(err.Error()),
	}
}

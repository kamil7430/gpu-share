package handler

import (
	"context"

	"github.com/kamil7430/gpu-share/gpu/coordinator/api"
	"github.com/kamil7430/gpu-share/gpu/coordinator/service"
)

type RestHandler struct {
	service.JobsService
}

var _ api.Handler = &RestHandler{}

func NewRestHandler() *RestHandler {
	return &RestHandler{
		service.NewJobsService(),
	}
}

func (s *RestHandler) GetHealth(ctx context.Context) (r *api.GetHealthOKHeaders, _ error) {
	return &api.GetHealthOKHeaders{}, nil
}

func (*RestHandler) NewError(ctx context.Context, err error) *api.ErrorStatusCode {
	return &api.ErrorStatusCode{
		StatusCode: 500,
		Response:   api.Error(err.Error()),
	}
}

package service

import (
	"context"
	"errors"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/ogen-go/ogen/ogenerrors"
)

type HealthService struct{}

func NewHealthService() HealthService {
	return HealthService{}
}

func (*HealthService) GetHealth(ctx context.Context) error {
	return nil
}

func (*HealthService) NewError(ctx context.Context, err error) *api.DefaultStatusCode {
	code := http.StatusInternalServerError
	if e, ok := errors.AsType[ogenerrors.Error](err); ok {
		code = e.Code()
	}

	return &api.DefaultStatusCode{
		StatusCode: code,
		Response:   api.Error(err.Error()),
	}
}

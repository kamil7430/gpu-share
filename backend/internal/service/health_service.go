package service

import (
	"context"
	"log"

	"github.com/kamil7430/gpu-share/backend/internal/api"
)

type HealthService struct{}

func NewHealthService() HealthService {
	return HealthService{}
}

func (*HealthService) GetHealth(ctx context.Context) error {
	return nil
}

func (*HealthService) NewError(ctx context.Context, err error) *api.DefaultStatusCode {
	log.Println(err)
	return &api.DefaultStatusCode{
		StatusCode: 500,
		Response:   api.Error(err.Error()),
	}
}

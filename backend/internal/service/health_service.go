package service

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/api"
)

type HealthService struct{}

func NewHealthService() HealthService {
	return HealthService{}
}

func (s *HealthService) GetHealth(ctx context.Context) (r *api.GetHealthOKHeaders, _ error) {
	return &api.GetHealthOKHeaders{}, nil
}

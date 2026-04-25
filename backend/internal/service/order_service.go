package service

import (
	"context"
	"errors"

	"github.com/kamil7430/gpu-share/backend/internal/api"
)

type OrderService struct{}

func NewOrderService() OrderService {
	return OrderService{}
}

func (s *OrderService) OrderDevice(ctx context.Context, params api.OrderDeviceParams) (api.OrderDeviceRes, error) {
	return nil, errors.New("not implemented")
}

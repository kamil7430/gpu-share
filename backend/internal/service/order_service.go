package service

import (
	"context"
	"errors"
	"math"
	"strconv"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
)

type OrderService struct {
	store repository.Store
}

func NewOrderService(store repository.Store) OrderService {
	return OrderService{
		store: store,
	}
}

func (s *OrderService) OrderDevice(ctx context.Context, params api.OrderDeviceParams) (api.OrderDeviceRes, error) {
	username, ok := ctx.Value(utils.ContextUsernameKey{}).(string)
	if !ok {
		return nil, errors.New("username not found in context")
	}

	device, err := s.store.Devices().GetDeviceById(ctx, params.DeviceId)
	if err != nil {
		return &api.OrderDeviceBadRequest{}, nil
	}

	renter, err := s.store.Users().GetUserByName(ctx, username)
	if err != nil {
		return nil, errors.New("user from context not found in repository")
	}

	if device.UserID == renter.ID {
		return &api.OrderDeviceBadRequest{}, nil
	}

	InternalError := errors.New("internal error")
	InsufficientBalanceError := errors.New("insufficient balance")
	BadRequest := errors.New("bad request")

	var order *model.Order

	err = s.store.Transaction(func(store repository.Store) error {
		device, err = store.Devices().GetDeviceById(ctx, params.DeviceId)
		if err != nil {
			return InternalError
		}
		if device.State != api.StateAVAILABLE {
			return BadRequest
		}

		renter, err = store.Users().GetUserByName(ctx, username)
		if err != nil {
			return InternalError
		}

		rentalCost := int(math.Ceil(float64(device.PricePerHourUsdCents) * params.DurationHours))
		if renter.WalletBalanceCents < rentalCost {
			return InsufficientBalanceError
		}

		order = &model.Order{
			DockerImage:     params.DockerImage,
			DurationHours:   float32(params.DurationHours),
			RentalStatus:    api.RentalStatusWAITINGFORSTART,
			RentalCostCents: rentalCost,
			UserID:          renter.ID,
			DeviceID:        device.ID,
		}

		err = store.Orders().AddOrder(ctx, order)
		if err != nil {
			return InternalError
		}

		renter.WalletBalanceCents -= rentalCost
		err = store.Users().UpdateUser(ctx, renter)
		if err != nil {
			return InternalError
		}

		device.State = api.StateRENTED
		err = store.Devices().UpdateDevice(ctx, device)
		if err != nil {
			return InternalError
		}

		return nil
	})
	if errors.Is(err, InternalError) {
		return nil, err
	} else if errors.Is(err, InsufficientBalanceError) {
		return &api.OrderDevicePaymentRequired{}, nil
	} else if errors.Is(err, BadRequest) {
		return &api.OrderDeviceBadRequest{}, nil
	}

	addresses, err := s.store.Gpus().GetConnectionDetailsById(ctx, strconv.Itoa(int(device.ID)))
	if err != nil {
		addresses = &model.ConnectionDetails{}
	}

	return &api.OrderDeviceCreated{
		OrderId: strconv.Itoa(int(order.ID)),
		Status:  order.RentalStatus,
		ConnectionDetails: api.ConnectionDetails{
			Host:     addresses.Host,
			Port:     addresses.Port,
			Protocol: addresses.Protocol,
		},
		TotalReservedCostCents: order.RentalCostCents,
	}, nil
}

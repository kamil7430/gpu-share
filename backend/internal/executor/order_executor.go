package executor

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"time"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
)

type OrderExecutor struct {
	store repository.Store
}

func NewOrderExecutor(store repository.Store) *OrderExecutor {
	return &OrderExecutor{
		store: store,
	}
}

func (e *OrderExecutor) Run(ctx context.Context) {
	log.Println("Executor started...")
	ticker := time.NewTicker(5 * time.Second)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return

		case <-ticker.C:
			e.processWaitingOrders(ctx)
		}
	}
}

func (e *OrderExecutor) processWaitingOrders(ctx context.Context) {
	orders, err := e.store.Orders().
		GetOrdersByStatus(ctx, api.RentalStatusWAITINGFORSTART)

	if err != nil {
		log.Printf("executor: failed to load orders: %v", err)
		return
	}

	for i := range orders {
		order := &orders[i]

		if err := e.startOrder(ctx, order); err != nil {
			log.Printf("executor: failed to start order %d: %v",
				order.ID, err)
		}
	}
}

func (e *OrderExecutor) startOrder(
	ctx context.Context,
	order *model.Order,
) error {
	order.RentalStatus = api.RentalStatusRUNNING

	if err := e.store.Orders().UpdateOrder(ctx, order); err != nil {
		return err
	}

	reqBody := map[string]any{
		"deviceId": order.DeviceID,
		"resources": map[string]any{
			"vRamMb":    1000,
			"cudaCores": 4000,
		},
		"maxTimeSec": 3600,
	}

	body, _ := json.Marshal(reqBody)

	envIp := utils.GetenvOrDefault("COORDINATOR_IP", "127.0.0.1")
	resp, err := http.Post(
		"http://"+envIp+"/jobs",
		"application/json",
		bytes.NewReader(body),
	)
	if err != nil {
		order.RentalStatus = api.RentalStatusFAILURE
		_ = e.store.Orders().UpdateOrder(ctx, order)
		return err
	}

	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		order.RentalStatus = api.RentalStatusFAILURE
		_ = e.store.Orders().UpdateOrder(ctx, order)
		return fmt.Errorf("coordinator returned %d", resp.StatusCode)
	}

	return nil
}

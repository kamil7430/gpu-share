package executor

import (
	"context"
	"fmt"
	"log"
	"net/http"
	"net/http/httputil"
	"strings"
	"time"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/service"
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

		if err := e.startOrder(ctx, order, e.store); err != nil {
			log.Printf("executor: failed to start order %d: %v",
				order.ID, err)
		}
	}
}

func (e *OrderExecutor) startOrder(
	ctx context.Context,
	order *model.Order,
	store repository.Store,
) error {
	order.RentalStatus = api.RentalStatusRUNNING

	if err := e.store.Orders().UpdateOrder(ctx, order); err != nil {
		return err
	}

	log.Printf("executing order id %v on device id %v\n", order.ID, order.DeviceID)

	reqBody := strings.NewReader(fmt.Sprintf(`{
		"deviceId": "%v",
		"resources": {
			"vRamMb": 1000,
			"cudaCores": 4000
		},
		"maxTimeSec": 360
	}`, order.DeviceID))

	sr := service.NewUserService(store)
	res, err := sr.Refresh(context.WithValue(ctx, utils.ContextUsernameKey{}, "user9"))
	if err != nil {
		return err
	}
	token := res.(*api.AuthToken).Token
	log.Println(token)

	coordIp := utils.GetenvOrDefault("GPU_IP", "127.0.0.1")
	coordPort := "2138"
	req, err := http.NewRequestWithContext(ctx, "POST", "http://"+coordIp+":"+coordPort+"/api/jobs", reqBody)

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Authorization", "test")
	dump, _ := httputil.DumpRequestOut(req, true)
	log.Printf("%s", dump)
	resp, err := http.DefaultClient.Do(req)

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

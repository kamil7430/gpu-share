package repository

import (
	"context"

	"github.com/go-faster/errors"
	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/gorm"
)

type OrderRepository interface {
	AddOrder(ctx context.Context, order *model.Order) error
	GetOrdersByUserId(ctx context.Context, userId uint, limit int) ([]model.Order, error)
	GetOrderById(ctx context.Context, id string) (model.Order, error)
	GetOrdersByStatus(ctx context.Context, status api.RentalStatus) ([]model.Order, error)
	UpdateOrder(ctx context.Context, order *model.Order) error
}

type orderRepository struct {
	db *gorm.DB
}

func (r *orderRepository) AddOrder(ctx context.Context, order *model.Order) error {
	return gorm.G[model.Order](r.db).Create(ctx, order)
}

func (r *orderRepository) GetOrdersByUserId(ctx context.Context, userId uint, limit int) ([]model.Order, error) {
	return gorm.G[model.Order](r.db).Where("user_id = ?", userId).Limit(limit).Find(ctx)
}

func (r *orderRepository) GetOrderById(ctx context.Context, id string) (model.Order, error) {
	return gorm.G[model.Order](r.db).Preload("Review", func(db gorm.PreloadBuilder) error { return nil }).Where("id = ?", id).First(ctx)
}

func (r *orderRepository) GetOrdersByStatus(
	ctx context.Context,
	status api.RentalStatus,
) ([]model.Order, error) {
	return gorm.G[model.Order](r.db).
		Where("rental_status = ?", status).
		Find(ctx)
}

func (r *orderRepository) UpdateOrder(
	ctx context.Context,
	order *model.Order,
) error {
	rowsAffected, err := gorm.G[model.Order](r.db).Where("id = ?", order.ID).Updates(ctx, *order)
	if err != nil {
		return err
	}
	if rowsAffected != 1 {
		return errors.New("affected rows is not equal to 1")
	}
	return nil
}

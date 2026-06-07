package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/gorm"
)

type OrderRepository interface {
	AddOrder(ctx context.Context, order *model.Order) error
	GetOrdersByUserId(ctx context.Context, userId uint, limit int) ([]model.Order, error)
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

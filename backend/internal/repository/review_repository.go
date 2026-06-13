package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/gorm"
)

type ReviewRepository interface {
	AddReview(ctx context.Context, review *model.Review) error
	GetReviewsByDeviceId(ctx context.Context, deviceId uint, limit int) ([]model.Review, error)
}

type reviewRepository struct {
	db *gorm.DB
}

func (r *reviewRepository) AddReview(ctx context.Context, review *model.Review) error {
	return gorm.G[model.Review](r.db).Create(ctx, review)
}

func (r *reviewRepository) GetReviewsByDeviceId(ctx context.Context, deviceId uint, limit int) ([]model.Review, error) {
	return gorm.G[model.Review](r.db).Raw("SELECT * FROM reviews LEFT JOIN orders ON orders.id = reviews.order_id WHERE orders.device_id = ? LIMIT ?", deviceId, limit).Find(ctx)
}

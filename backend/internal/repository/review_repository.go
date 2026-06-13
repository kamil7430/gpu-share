package repository

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/gorm"
)

type ReviewRepository interface {
	AddReview(ctx context.Context, review *model.Review) error
}

type reviewRepository struct {
	db *gorm.DB
}

func (r *reviewRepository) AddReview(ctx context.Context, review *model.Review) error {
	return gorm.G[model.Review](r.db).Create(ctx, review)
}

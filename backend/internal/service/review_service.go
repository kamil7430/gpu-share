package service

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
)

type ReviewService struct {
	store repository.Store
}

func NewReviewService(store repository.Store) ReviewService {
	return ReviewService{
		store: store,
	}
}

func (s *ReviewService) GetReviewsByDeviceId(ctx context.Context, params api.GetReviewsByDeviceIdParams) (api.GetReviewsByDeviceIdRes, error) {
	panic("unimplemented")
}

func (s *ReviewService) GetReviewsByUsername(ctx context.Context, params api.GetReviewsByUsernameParams) (api.GetReviewsByUsernameRes, error) {
	panic("unimplemented")
}

func (s *ReviewService) GetUserRating(ctx context.Context, params api.GetUserRatingParams) (api.GetUserRatingRes, error) {
	panic("unimplemented")
}

func (s *ReviewService) ReviewOrderById(ctx context.Context, req *api.ReviewOrderByIdReq, params api.ReviewOrderByIdParams) (api.ReviewOrderByIdRes, error) {
	panic("unimplemented")
}

package service

import (
	"context"
	"errors"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
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
	username, ok := ctx.Value(utils.ContextUsernameKey{}).(string)
	if !ok {
		return nil, errors.New("username not found in context")
	}

	user, err := s.store.Users().GetUserByName(ctx, username)
	if err != nil {
		return nil, err
	}

	order, err := s.store.Orders().GetOrderById(ctx, params.OrderId)
	if err != nil {
		return &api.ReviewOrderByIdBadRequest{}, nil
	}

	if order.UserID != user.ID {
		return &api.ReviewOrderByIdUnauthorized{}, nil
	}

	if order.Review != nil {
		return &api.ReviewOrderByIdConflict{}, nil
	}

	review := model.Review{
		Rating:  req.Rating,
		Comment: req.Comment,
		OrderID: order.ID,
	}

	err = s.store.Reviews().AddReview(ctx, &review)
	if err != nil {
		return nil, err
	}

	return &api.ReviewOrderByIdCreated{
		ReviewId:       int(review.ID),
		AuthorUsername: username,
		CreatedAt:      review.CreatedAt,
	}, nil
}

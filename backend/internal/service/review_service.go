package service

import (
	"context"
	"errors"
	"strconv"

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
	device, err := s.store.Devices().GetDeviceById(ctx, params.DeviceId)
	if err != nil {
		return &api.GetReviewsByDeviceIdNotFound{}, nil
	}

	reviews, err := s.store.Reviews().GetReviewsByDeviceId(ctx, device.ID, params.Limit.Value)
	if err != nil {
		return nil, err
	}

	rev := make(api.GetReviewsByDeviceIdOKApplicationJSON, 0, len(reviews))
	for _, review := range reviews {
		newRev := api.Review{
			ReviewId:       int(review.ID),
			OrderId:        strconv.Itoa(int(review.OrderID)),
			AuthorUsername: review.AuthorUsername,
			Rating:         review.Rating,
			Comment: api.OptString{
				Value: review.Comment,
				Set:   true,
			},
			CreatedAt: review.CreatedAt,
		}

		rev = append(rev, newRev)
	}

	return &rev, nil
}

func (s *ReviewService) GetReviewsByUsername(ctx context.Context, params api.GetReviewsByUsernameParams) (api.GetReviewsByUsernameRes, error) {
	user, err := s.store.Users().GetUserByName(ctx, params.Username)
	if err != nil {
		return nil, err
	}

	devices, err := s.store.Devices().GetDevicesForUser(ctx, user.ID, api.GetDevicesParams{})
	if err != nil {
		return nil, err
	}

	reviews := make(api.GetReviewsByUsernameOKApplicationJSON, 0, params.Limit.Value)
	for _, device := range devices {
		revs, err := s.GetReviewsByDeviceId(ctx, api.GetReviewsByDeviceIdParams{
			DeviceId: strconv.Itoa(int(device.ID)),
		})
		if err != nil {
			return nil, err
		}

		if targetType, ok := revs.(*api.GetReviewsByDeviceIdOKApplicationJSON); ok && targetType != nil {
			cast := []api.Review(*targetType)

			for _, rev := range cast {
				if len(reviews) < params.Limit.Value {
					reviews = append(reviews, rev)
				} else {
					return &reviews, nil
				}
			}
		}
	}

	return &reviews, nil
}

func (s *ReviewService) GetUserRating(ctx context.Context, params api.GetUserRatingParams) (api.GetUserRatingRes, error) {
	user, err := s.store.Users().GetUserByName(ctx, params.Username)
	if err != nil {
		return &api.GetUserRatingNotFound{}, nil
	}

	revs, err := s.GetReviewsByUsername(ctx, api.GetReviewsByUsernameParams{
		Username: user.Name,
	})
	if err != nil {
		return nil, err
	}

	sumRatings := 0
	ratingCount := 0

	if targetType, ok := revs.(*api.GetReviewsByUsernameOKApplicationJSON); ok && targetType != nil {
		cast := []api.Review(*targetType)

		for _, rev := range cast {
			sumRatings += rev.Rating
			ratingCount += 1
		}
	}

	if ratingCount > 0 {
		return &api.GetUserRatingOK{
			AverageRating: float32(sumRatings) / float32(ratingCount),
			RatingCount:   ratingCount,
		}, nil
	}

	return &api.GetUserRatingOK{}, nil
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
		AuthorUsername: username,
		Rating:         req.Rating,
		Comment:        req.Comment,
		OrderID:        order.ID,
	}

	err = s.store.Reviews().AddReview(ctx, &review)
	if err != nil {
		//if errors.Is(err, gorm.ErrDuplicatedKey) {
		//	return &api.ReviewOrderByIdConflict{}, nil
		//}
		return nil, err
	}

	return &api.ReviewOrderByIdCreated{
		ReviewId:       int(review.ID),
		AuthorUsername: review.AuthorUsername,
		CreatedAt:      review.CreatedAt,
	}, nil
}

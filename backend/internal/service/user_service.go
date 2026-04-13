package service

import (
	"context"
	"errors"
	"slices"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
)

type UserService struct {
	ur repository.UserRepository
}

func NewUserService(ur repository.UserRepository) UserService {
	return UserService{ur}
}

func (s *UserService) HandleBearerAuth(ctx context.Context, operationName api.OperationName, t api.BearerAuth) (context.Context, error) {
	token, err := auth.ParseToken(t.Token)
	if err != nil {
		return nil, err
	}

	if slices.Contains(t.Roles, "user") {
		return ctx, nil
	} else if slices.Contains(t.Roles, "admin") {
		if token.Admin {
			return ctx, nil
		}
		return nil, errors.New("forbidden")
	}

	panic("unreachable")
}

func (s *UserService) Login(ctx context.Context, req *api.LoginReq) (api.LoginRes, error) {
	panic("todo")
}

func (s *UserService) Register(ctx context.Context, req *api.RegisterReq) (api.RegisterRes, error) {
	panic("todo")
}

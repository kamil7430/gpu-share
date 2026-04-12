package service

import (
	"context"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
)

type UserService struct {
	ur repository.UserRepository
}

func NewUserService(ur repository.UserRepository) UserService {
	return UserService{ur}
}

func (s *UserService) HandleBearerAuth(ctx context.Context, operationName api.OperationName, t api.BearerAuth) (context.Context, error) {
	//TODO implement me
	panic("implement me")
}

package service

import (
	"context"
	"errors"
	"slices"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
	"gorm.io/gorm"
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

	newCtx := context.WithValue(ctx, utils.ContextUsernameKey, token.Username)

	if slices.Contains(t.Roles, "user") {
		return newCtx, nil
	} else if slices.Contains(t.Roles, "admin") {
		if token.Admin {
			return newCtx, nil
		}
		return nil, errors.New("forbidden")
	}

	panic("unreachable")
}

func (s *UserService) Login(ctx context.Context, req *api.LoginReq) (api.LoginRes, error) {
	user, err := s.ur.GetUserByName(ctx, req.Username)
	if err != nil {
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return &api.LoginNotFound{}, nil
		}
		return nil, err
	}

	err = auth.CompareHashAndPassword(user.Password, req.Password)
	if err != nil {
		return &api.LoginUnauthorized{}, nil
	}

	token, err := auth.CreateToken(user)
	if err != nil {
		return nil, err
	}

	return &api.AuthToken{
		Token: token,
	}, nil
}

func (s *UserService) Register(ctx context.Context, req *api.RegisterReq) (api.RegisterRes, error) {
	if _, err := s.ur.GetUserByName(ctx, req.Username); err == nil {
		return &api.RegisterConflict{}, nil
	}

	if err := auth.ValidateUsername(req.Username); err != nil {
		errResp := api.RegisterBadRequestApplicationJSON(err.Error())
		return &errResp, nil
	}
	if err := auth.ValidatePassword(req.Password); err != nil {
		errResp := api.RegisterBadRequestApplicationJSON(err.Error())
		return &errResp, nil
	}

	hash, err := auth.HashPassword(req.Password)
	if err != nil {
		return nil, err
	}

	user := &model.User{
		Name:     req.Username,
		Password: string(hash),
		Admin:    false,
	}
	err = s.ur.AddUser(ctx, user)
	if err != nil {
		return nil, err
	}

	token, err := auth.CreateToken(user)
	if err != nil {
		return nil, err
	}

	return &api.AuthToken{
		Token: token,
	}, nil
}

func (s *UserService) ChangePassword(ctx context.Context, req *api.ChangePasswordReq) (api.ChangePasswordRes, error) {
	username, ok := ctx.Value(utils.ContextUsernameKey).(string)
	if !ok {
		return nil, errors.New("username not found in context")
	}

	user, err := s.ur.GetUserByName(ctx, username)
	if err != nil {
		return nil, err
	}

	if err := auth.ValidatePassword(req.NewPassword); err != nil {
		return &api.ChangePasswordBadRequest{}, nil
	}

	err = auth.CompareHashAndPassword(user.Password, req.OldPassword)
	if err != nil {
		return &api.ChangePasswordUnauthorized{}, nil
	}

	if req.OldPassword == req.NewPassword {
		return &api.ChangePasswordBadRequest{}, nil
	}

	newPasswordHash, err := auth.HashPassword(req.NewPassword)
	if err != nil {
		return nil, err
	}

	user.Password = string(newPasswordHash)
	err = s.ur.UpdateUser(ctx, user)
	if err != nil {
		return nil, err
	}

	return &api.ChangePasswordOK{}, nil
}

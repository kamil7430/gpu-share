package repository

import (
	"context"
	"errors"

	"github.com/kamil7430/gpu-share/backend/internal/model"
)

type UserRepository interface {
	AddUser(ctx context.Context, user *model.User) error
	GetUserByLogin(ctx context.Context, login string) (*model.User, error)
	UpdateUser(ctx context.Context, user *model.User) error
}

type mockUserRepository struct {
	users map[string]*model.User
}

func NewMockUserRepository() UserRepository {
	return &mockUserRepository{}
}

func (r *mockUserRepository) AddUser(ctx context.Context, user *model.User) error {
	if _, ok := r.users[user.Name]; ok {
		return errors.New("this user already exists")
	}

	r.users[user.Name] = user
	return nil
}

func (r *mockUserRepository) GetUserByLogin(ctx context.Context, login string) (*model.User, error) {
	user, ok := r.users[login]
	if !ok {
		return nil, errors.New("this user does not exist")
	}
	return user, nil
}

func (r *mockUserRepository) UpdateUser(ctx context.Context, user *model.User) error {
	if _, ok := r.users[user.Name]; !ok {
		return errors.New("this user does not exist")
	}

	r.users[user.Name] = user
	return nil
}

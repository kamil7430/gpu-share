package repository

import (
	"context"
	"errors"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"golang.org/x/crypto/bcrypt"
)

type UserRepository interface {
	AddUser(ctx context.Context, user *model.User) error
	GetUserByName(ctx context.Context, name string) (*model.User, error)
	UpdateUser(ctx context.Context, user *model.User) error
}

type mockUserRepository struct {
	users map[string]*model.User
}

func NewMockUserRepository() UserRepository {
	users := make(map[string]*model.User)

	normalPassword, err := bcrypt.GenerateFromPassword([]byte("NormalPassword"), bcrypt.DefaultCost)
	if err != nil {
		panic(err)
	}
	users["NormalUser"] = &model.User{
		Name:     "NormalUser",
		Password: string(normalPassword),
		Admin:    false,
	}

	adminPassword, err := bcrypt.GenerateFromPassword([]byte("AdminPassword"), bcrypt.DefaultCost)
	if err != nil {
		panic(err)
	}
	users["AdminUser"] = &model.User{
		Name:     "AdminUser",
		Password: string(adminPassword),
		Admin:    true,
	}

	return &mockUserRepository{users}
}

func (r *mockUserRepository) AddUser(ctx context.Context, user *model.User) error {
	if _, ok := r.users[user.Name]; ok {
		return errors.New("this user already exists")
	}

	r.users[user.Name] = user
	return nil
}

func (r *mockUserRepository) GetUserByName(ctx context.Context, name string) (*model.User, error) {
	user, ok := r.users[name]
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

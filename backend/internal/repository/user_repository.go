package repository

import (
	"context"
	"fmt"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/gorm"
)

type UserRepository interface {
	AddUser(ctx context.Context, user *model.User) error
	GetUserByName(ctx context.Context, name string) (*model.User, error)
	UpdateUser(ctx context.Context, user *model.User) error
}

type userRepository struct {
	db *gorm.DB
}

func NewUserRepository(db *gorm.DB) UserRepository {
	return &userRepository{db}
}

func (r *userRepository) AddUser(ctx context.Context, user *model.User) error {
	return gorm.G[model.User](r.db).Create(ctx, user)
}

func (r *userRepository) GetUserByName(ctx context.Context, name string) (*model.User, error) {
	user, err := gorm.G[model.User](r.db).Where("Name = ?", name).First(ctx)
	if err != nil {
		return nil, err
	}
	return &user, nil
}

func (r *userRepository) UpdateUser(ctx context.Context, user *model.User) error {
	rowsAffected, err := gorm.G[model.User](r.db).Where("Name = ?", user.Name).Updates(ctx, *user)
	if rowsAffected != 1 {
		return fmt.Errorf("invalid affected rows count: %d", rowsAffected)
	}
	return err
}

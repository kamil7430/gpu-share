package repository

import "gorm.io/gorm"

type Store interface {
	Users() UserRepository
	Devices() DeviceRepository
	Gpus() GpuRepository
	Orders() OrderRepository
	Transaction(fn func(Store) error) error
}

type store struct {
	db *gorm.DB
}

func NewStore(db *gorm.DB) Store {
	return store{db}
}

func (s store) Users() UserRepository {
	return &userRepository{db: s.db}
}

func (s store) Devices() DeviceRepository {
	return &deviceRepository{db: s.db}
}

func (s store) Gpus() GpuRepository {
	// TODO: replace with actual implementation once finished
	return NewMockGpuRepository()
}

func (s store) Orders() OrderRepository {
	return &orderRepository{db: s.db}
}

func (s store) Transaction(fn func(Store) error) error {
	return s.db.Transaction(func(tx *gorm.DB) error {
		return fn(store{db: tx})
	})
}

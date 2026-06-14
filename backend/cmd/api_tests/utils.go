package api_tests

import (
	"errors"

	"gorm.io/gorm"
)

func truncateTables(db *gorm.DB) {
	db.Exec("TRUNCATE TABLE devices, users, orders, reviews;")
}

func remove[T comparable](slice []T, element T) ([]T, error) {
	for i, elem := range slice {
		if elem == element {
			if i == 0 {
				return slice[1:], nil
			} else if i == len(slice)-1 {
				return slice[:len(slice)-1], nil
			}
			return append(slice[:i], slice[i+1:]...), nil
		}
	}

	return nil, errors.New("element not found in slice")
}

package model

import "gorm.io/gorm"

type Review struct {
	gorm.Model
	Rating  int
	Comment string
	OrderID uint
}

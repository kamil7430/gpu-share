package model

import "gorm.io/gorm"

type Review struct {
	gorm.Model
	AuthorUsername string
	Rating         int
	Comment        string
	OrderID        uint
}

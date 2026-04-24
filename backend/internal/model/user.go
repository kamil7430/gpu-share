package model

import "gorm.io/gorm"

type User struct {
	gorm.Model
	Name     string
	Password string
	Admin    bool
	Devices  []Device
}

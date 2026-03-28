package service

import (
	"fmt"
)

type UserService struct {}

func (u *UserService) ValidatePassword(username string, password string) error {
	if username != "Maklowicz" {
		return fmt.Errorf("username not found")
	}

	if password != "żwir" {
		return fmt.Errorf("invalid password")
	}

	return nil
}

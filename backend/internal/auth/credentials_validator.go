package auth

import (
	"fmt"
	"regexp"
)

func ValidateUsername(username string) error {
	const minUsernameLength = 4
	const maxUsernameLength = 40
	const allowedCharactersRegex = "^[a-zA-Z0-9_]*$"

	if len(username) < minUsernameLength || len(username) > maxUsernameLength {
		return fmt.Errorf("username should be between %d and %d characters", minUsernameLength, maxUsernameLength)
	}

	regex := regexp.MustCompile(allowedCharactersRegex)
	if !regex.MatchString(username) {
		return fmt.Errorf("username must only contain alphanumeric characters")
	}

	return nil
}

func ValidatePassword(password string) error {
	const minPasswordLength = 8
	const maxPasswordLength = 128

	if len(password) < minPasswordLength || len(password) > maxPasswordLength {
		return fmt.Errorf("password should be between %d and %d characters", minPasswordLength, maxPasswordLength)
	}

	return nil
}

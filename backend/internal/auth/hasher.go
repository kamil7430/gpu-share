package auth

import (
	"crypto/sha256"
	"fmt"

	"golang.org/x/crypto/bcrypt"
)

const HashingCost = bcrypt.DefaultCost

func HashPassword(password string) ([]byte, error) {
	sum256 := sha256.Sum256([]byte(password))
	hashed := fmt.Sprintf("%s", sum256) // IDK how to cast [32]byte to []byte xD
	return bcrypt.GenerateFromPassword([]byte(hashed), HashingCost)
}

func CompareHashAndPassword(passwordHash, plaintext string) error {
	sum256 := sha256.Sum256([]byte(plaintext))
	hashed := fmt.Sprintf("%s", sum256)
	return bcrypt.CompareHashAndPassword([]byte(passwordHash), []byte(hashed))
}

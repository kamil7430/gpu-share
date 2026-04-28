package auth

import (
	"crypto/sha256"

	"golang.org/x/crypto/bcrypt"
)

const HashingCost = bcrypt.DefaultCost

func HashPassword(password string) ([]byte, error) {
	sum256 := sha256.Sum256([]byte(password))
	return bcrypt.GenerateFromPassword(sum256[:], HashingCost)
}

func CompareHashAndPassword(passwordHash, plaintext string) error {
	sum256 := sha256.Sum256([]byte(plaintext))
	return bcrypt.CompareHashAndPassword([]byte(passwordHash), sum256[:])
}

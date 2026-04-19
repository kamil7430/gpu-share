package auth

import (
	"errors"
	"os"
	"time"

	"github.com/golang-jwt/jwt/v5"
	"github.com/kamil7430/gpu-share/backend/internal/model"
)

const tokenValidity = 10 * time.Minute

var secretKey = func() []byte {
	token := os.Getenv("JWT_SECRET_KEY")
	if token == "" {
		panic("JWT_SECRET_KEY not present or empty")
	}
	return []byte(token)
}()

type jwtClaims struct {
	Username string `json:"username"`
	Admin    bool   `json:"admin"`
	jwt.RegisteredClaims
}

func CreateToken(user *model.User) (string, error) {
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, jwtClaims{
		Username: user.Name,
		Admin:    user.Admin,
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(time.Now().Add(tokenValidity)),
			IssuedAt:  jwt.NewNumericDate(time.Now()),
		},
	})

	tokenString, err := token.SignedString(secretKey)
	if err != nil {
		return "", err
	}

	return tokenString, nil
}

func ParseToken(tokenString string) (*Token, error) {
	token, err := jwt.ParseWithClaims(tokenString, &jwtClaims{}, func(token *jwt.Token) (any, error) {
		return secretKey, nil
	})
	if err != nil {
		return nil, err
	}
	if !token.Valid {
		return nil, errors.New("invalid token")
	}

	claims, ok := token.Claims.(*jwtClaims)
	if !ok {
		return nil, errors.New("invalid token")
	}

	return &Token{
		Username: claims.Username,
		Admin:    claims.Admin,
	}, nil
}

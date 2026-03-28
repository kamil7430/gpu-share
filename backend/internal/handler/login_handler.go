package handler

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/kamil7430/gpu-share/backend/internal/service"
)

type LoginHandler struct {
	userService *service.UserService
}

func NewLoginHandler(userService *service.UserService) *LoginHandler {
	return &LoginHandler{userService}
}

func (l *LoginHandler) Login(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")

	var u struct {
		Username string `json:"username"`
		Password string `json:"password"`
	}
	json.NewDecoder(r.Body).Decode(&u)

	log.Print(u)
	if err := l.userService.ValidatePassword(u.Username, u.Password); err != nil {
		w.WriteHeader(http.StatusUnauthorized)
		fmt.Fprint(w, err.Error())
		return
	}

	tokenString, err := auth.CreateToken(u.Username)
	if err != nil {
		w.WriteHeader(http.StatusInternalServerError)
		fmt.Fprint(w, "couldn't create token")
		return
	}

	w.WriteHeader(http.StatusOK)
	fmt.Fprint(w, tokenString)
}

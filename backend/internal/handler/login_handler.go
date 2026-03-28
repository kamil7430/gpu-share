package handler

import (
	"encoding/json"
	"fmt"
	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"net/http"
)

// temporary
type User struct {
	Username string
	Password string
}

func LoginHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")

	var u User
	json.NewDecoder(r.Body).Decode(&u)
	fmt.Printf("The user request value %v", u)

	if u.Username == "Maklowicz" && u.Password == "żwir" {
		tokenString, err := auth.CreateToken(u.Username)
		if err != nil {
			w.WriteHeader(http.StatusInternalServerError)
			fmt.Fprint(w, "No username found")
		}
		w.WriteHeader(http.StatusOK)
		fmt.Fprint(w, tokenString)
		return
	} else {
		w.WriteHeader(http.StatusUnauthorized)
		fmt.Fprint(w, "Invalid credentials")
	}
}

package handler

import (
	"bytes"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/service"
	"github.com/stretchr/testify/require"
)

func TestLogin(t *testing.T) {
	user := struct {
		Username string `json:"username"`
		Password string `json:"password"`
	}{
		Username: "Maklowicz",
		Password: "żwir",
	}
	body, err := json.Marshal(user)
	require.Nil(t, err)

	req := httptest.NewRequest(http.MethodPost, "/login", bytes.NewReader(body))
	w := httptest.NewRecorder()

	h := NewLoginHandler(&service.UserService{})

	h.Login(w, req)

	res := w.Result()
	defer res.Body.Close()

	if res.StatusCode != http.StatusOK {
		t.Fatalf("expected 200, got %d", res.StatusCode)
	}

	require.NotEmpty(t, res.Body)
}

func TestLogin_InavlidUsername(t *testing.T) {
	user := struct {
		Username string `json:"username"`
		Password string `json:"password"`
	}{
		Username: "wcale nie Maklowicz",
		Password: "żwir",
	}
	body, err := json.Marshal(user)
	require.Nil(t, err)

	req := httptest.NewRequest(http.MethodPost, "/login", bytes.NewReader(body))
	w := httptest.NewRecorder()

	h := NewLoginHandler(&service.UserService{})

	h.Login(w, req)

	res := w.Result()
	defer res.Body.Close()

	if res.StatusCode != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", res.StatusCode)
	}

	require.NotEmpty(t, res.Body)
}

func TestLogin_InavlidPassword(t *testing.T) {
	user := struct {
		Username string `json:"username"`
		Password string `json:"password"`
	}{
		Username: "Maklowicz",
		Password: "2137",
	}
	body, err := json.Marshal(user)
	require.Nil(t, err)

	req := httptest.NewRequest(http.MethodPost, "/login", bytes.NewReader(body))
	w := httptest.NewRecorder()

	h := NewLoginHandler(&service.UserService{})

	h.Login(w, req)

	res := w.Result()
	defer res.Body.Close()

	if res.StatusCode != http.StatusUnauthorized {
		t.Fatalf("expected 401, got %d", res.StatusCode)
	}

	require.NotEmpty(t, res.Body)
}

package api_tests

import (
	"fmt"
	"io"
	"net/http"
	"strings"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func testLogin(t *testing.T, db *gorm.DB, baseUrl string) {
	resetDbContent := func() {
		db.Exec("TRUNCATE TABLE users;")
		db.Exec("INSERT INTO users(name, password, admin) VALUES "+
			"('TestUser', ?, 'false'), ('TestAdmin', ?, 'true');",
			`$2a$10$VEJHquwA/Rs7wA3rLwl/oOTxtvJUoEcbaGVZpQD/tthdi92jatgMe`,
			`$2a$10$K8BWpClmpDBHR2RwFBBzzuar1E8Xw6ia//83W13FPJPHyLWB8djDS`)
	}

	loginTestCase := func(username string, password string) *http.Response {
		resetDbContent()

		payloadReader := strings.NewReader(fmt.Sprintf(`{
			"username": "%s",
			"password": "%s"
		}`, username, password))
		resp, err := http.Post(baseUrl+"/api/login", "application/json", payloadReader)

		require.NoError(t, err)
		return resp
	}

	t.Run("login -- normal user", func(t *testing.T) {
		resp := loginTestCase("TestUser", "TestPassword")
		require.Equal(t, http.StatusOK, resp.StatusCode)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		token, err := auth.ParseToken(string(body))
		require.NoError(t, err)
		require.Equal(t, "TestUser", token.Username)
		require.Equal(t, false, token.Admin)
	})

	t.Run("login -- admin user", func(t *testing.T) {
		resp := loginTestCase("TestAdmin", "TestAdminPassword")
		require.Equal(t, http.StatusOK, resp.StatusCode)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		token, err := auth.ParseToken(string(body))
		require.NoError(t, err)
		require.Equal(t, "TestAdmin", token.Username)
		require.Equal(t, true, token.Admin)
	})

	t.Run("login -- invalid user", func(t *testing.T) {
		resp := loginTestCase("TestInvalidUser", "TestInvalidPassword")
		require.Equal(t, http.StatusNotFound, resp.StatusCode)
	})

	t.Run("login -- invalid password", func(t *testing.T) {
		resp := loginTestCase("TestUser", "TestInvalidPassword")
		require.Equal(t, http.StatusUnauthorized, resp.StatusCode)
	})
}

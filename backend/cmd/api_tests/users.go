package api_tests

import (
	"fmt"
	"io"
	"net/http"
	"strings"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/ogen-go/ogen/json"
	"github.com/stretchr/testify/require"
	"golang.org/x/crypto/bcrypt"
	"gorm.io/gorm"
)

type tokenResponse struct {
	Token string
}

func testLogin(t *testing.T, db *gorm.DB, baseUrl string) {
	userPassword, err := bcrypt.GenerateFromPassword([]byte("TestUserPassword"), bcrypt.DefaultCost)
	require.NoError(t, err)
	adminPassword, err := bcrypt.GenerateFromPassword([]byte("TestAdminPassword"), bcrypt.DefaultCost)
	require.NoError(t, err)

	resetDbContent := func() {
		db.Exec("TRUNCATE TABLE users;")
		db.Exec("INSERT INTO users(name, password, admin) VALUES ('TestUser', ?, 'false'), ('TestAdmin', ?, 'true');",
			userPassword, adminPassword)
	}

	loginTestCase := func(username, password string) *http.Response {
		resetDbContent()

		payloadReader := strings.NewReader(fmt.Sprintf(`{
			"username": "%s",
			"password": "%s"
		}`, username, password))
		resp, err := http.Post(baseUrl+"/api/users/login", "application/json", payloadReader)

		require.NoError(t, err)
		return resp
	}

	t.Run("login -- normal user", func(t *testing.T) {
		resp := loginTestCase("TestUser", "TestUserPassword")
		require.Equal(t, http.StatusOK, resp.StatusCode)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		var tokenObj tokenResponse
		err = json.Unmarshal(body, &tokenObj)
		require.NoError(t, err)

		token, err := auth.ParseToken(tokenObj.Token)
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

		var tokenObj tokenResponse
		err = json.Unmarshal(body, &tokenObj)
		require.NoError(t, err)

		token, err := auth.ParseToken(tokenObj.Token)
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

func testRegister(t *testing.T, db *gorm.DB, baseUrl string) {
	resetDbContent := func() {
		db.Exec("TRUNCATE TABLE users;")
	}

	registerTestCase := func(username, password string) *http.Response {
		resetDbContent()

		payloadReader := strings.NewReader(fmt.Sprintf(`{
			"username": "%s",
			"password": "%s"
		}`, username, password))
		resp, err := http.Post(baseUrl+"/api/users/register", "application/json", payloadReader)

		require.NoError(t, err)
		return resp
	}

	t.Run("register -- valid user, should register", func(t *testing.T) {
		resp := registerTestCase("TestUser1", "TestPassword1")
		require.Equal(t, http.StatusCreated, resp.StatusCode)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		var tokenObj tokenResponse
		err = json.Unmarshal(body, &tokenObj)
		require.NoError(t, err)

		token, err := auth.ParseToken(tokenObj.Token)
		require.NoError(t, err)
		require.Equal(t, "TestUser1", token.Username)
		require.Equal(t, false, token.Admin)
	})

	t.Run("register -- existing username, should fail", func(t *testing.T) {
		resp := registerTestCase("TestUser1", "TestPassword1")
		require.Equal(t, http.StatusCreated, resp.StatusCode)
		resp = registerTestCase("TestUser1", "TestPassword2")
		require.Equal(t, http.StatusConflict, resp.StatusCode)
	})

	t.Run("register -- too short password", func(t *testing.T) {
		resp := registerTestCase("TestUser", "12345")
		require.Equal(t, http.StatusBadRequest, resp.StatusCode)
	})

	t.Run("register -- too long password", func(t *testing.T) {
		resp := registerTestCase("TestUser", "12345678901234567890") // 20 chars
		require.Equal(t, http.StatusBadRequest, resp.StatusCode)
	})

	t.Run("register -- too short username", func(t *testing.T) {
		resp := registerTestCase("A", "123456789")
		require.Equal(t, http.StatusBadRequest, resp.StatusCode)
	})

	t.Run("register -- too long username", func(t *testing.T) {
		resp := registerTestCase("12345678901234567890123456789012345678901", "123456789") // 41 chars
		require.Equal(t, http.StatusBadRequest, resp.StatusCode)
	})

	t.Run("register -- forbidden chars in username", func(t *testing.T) {
		resp := registerTestCase("User@1234", "123456789")
		require.Equal(t, http.StatusBadRequest, resp.StatusCode)
	})
}

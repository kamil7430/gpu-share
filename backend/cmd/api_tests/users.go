package api_tests

import (
	"fmt"
	"net/http"
	"strings"
	"testing"

	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func testLogin(t *testing.T, db *gorm.DB, baseUrl string) {
	resetDbContent := func() {
		db.Exec("TRUNCATE TABLE users;")
		db.Exec("INSERT INTO users(name, password, admin) VALUES " +
			"('TestUser', '$2a$10$VEJHquwA/Rs7wA3rLwl/oOTxtvJUoEcbaGVZpQD/tthdi92jatgMe', 'false'), " +
			"('TestAdmin', '$2a$10$K8BWpClmpDBHR2RwFBBzzuar1E8Xw6ia//83W13FPJPHyLWB8djDS', 'true'), ")
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

	})
}

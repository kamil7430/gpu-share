package api_tests

import (
	"io"
	"net/http"
	"strings"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func testOrderDevice(t *testing.T, db *gorm.DB, baseUrl string) {
	testUserPassword, err := auth.HashPassword("TestPassword")
	require.NoError(t, err)

	resetDbContent := func() {
		truncateTables(db)
		db.Exec("INSERT INTO users(name, password, admin) VALUES ('TestOwner', ?, 'false');", testUserPassword)
		db.Exec("INSERT INTO users(name, password, admin) VALUES ('TestRentingUser', ?, 'false');", testUserPassword)
		db.Exec()
	}

	t.Run("device rent", func(t *testing.T) {
		payload := `{
			"device_id": "550e8400-e29b-41d4-a716-446655440000",
			"docker_image": "pytorch/pytorch:2.0-cuda11.7",
			"duration_hours": 2
		}`

		response, err := http.Post(baseUrl+"/api/orders", "application/json", strings.NewReader(payload))
		require.NoError(t, err)
		defer response.Body.Close()

		body, err := io.ReadAll(response.Body)
		require.NoError(t, err)

		expected := `{
			"order_id": "ord_123456789",
			"status": "WAITING_FOR_START",
			"connection_details": {
				"host": "node-01.gpushare.net",
				"port": 443,
				"protocol": "wss"
			},
			"total_reserved_cost": 0.90
		}`

		require.JSONEq(t, expected, string(body))
	})
}

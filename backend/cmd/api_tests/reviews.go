package api_tests

import (
	"io"
	"net/http"
	"strings"
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/ogen-go/ogen/json"
	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

func testReviewService(t *testing.T, db *gorm.DB, baseUrl string) {
	testUserPassword, err := auth.HashPassword("TestPassword")
	require.NoError(t, err)

	truncateTables(db)
	db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (100, 'TestOwner', ?, 'false', 0);", testUserPassword)
	db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (101, 'TestRentingUser1', ?, 'false', 10000);", testUserPassword)
	db.Exec("INSERT INTO users(id, name, password, admin, wallet_balance_cents) VALUES (102, 'TestRentingUser2', ?, 'false', 10);", testUserPassword)
	db.Exec("INSERT INTO devices(id, name, gpu_model, vram_mb, cuda_cores, price_per_hour_usd_cents, driver_version_major, driver_version_minor, state, user_id) " +
		"VALUES ('1', 'TestCard', 'NVIDIA GeForce RTX 3050', '8192', '2560', '1599', '595', '97', 'AVAILABLE', '100');")

	// placing order
	var renterToken string
	var orderId string
	{
		loginResp, err := http.Post(baseUrl+"/api/users/login", "application/json", strings.NewReader(`{
			"username": "TestRentingUser1",
			"password": "TestPassword"
		}`))
		require.NoError(t, err)
		require.Equal(t, http.StatusOK, loginResp.StatusCode)
		defer loginResp.Body.Close()

		body, err := io.ReadAll(loginResp.Body)
		require.NoError(t, err)

		var tokenObj tokenResponse
		err = json.Unmarshal(body, &tokenObj)
		require.NoError(t, err)

		renterToken = tokenObj.Token

		payloadReader := strings.NewReader(`{
			"deviceId": "1",
			"dockerImage": "pytorch/pytorch:2.0-cuda11.7",
			"durationHours": 2
		}`)
		req, err := http.NewRequestWithContext(t.Context(), "POST", baseUrl+"/api/orders", payloadReader)
		require.NoError(t, err)

		req.Header.Set("Content-Type", "application/json")
		req.Header.Set("Authorization", "Bearer "+renterToken)
		resp, err := http.DefaultClient.Do(req)
		require.NoError(t, err)
		require.Equal(t, http.StatusCreated, resp.StatusCode)
		defer resp.Body.Close()

		type responseStruct struct {
			OrderId                string
			ConnectionDetails      model.ConnectionDetails
			TotalReservedCostCents int
		}

		body, err = io.ReadAll(resp.Body)
		require.NoError(t, err)

		var respStruct responseStruct
		err = json.Unmarshal(body, &respStruct)
		require.NoError(t, err)

		orderId = respStruct.OrderId
	}

	type ratingResponse struct {
		AverageRating float32
		RatingCount   int
	}

	// testing reviews
	t.Run("reviews -- user Rating on zero reviews", func(t *testing.T) {
		resp, err := http.Get(baseUrl + "/api/reviews/userRating/TestOwner")
		require.NoError(t, err)
		require.Equal(t, http.StatusOK, resp.StatusCode)
		defer resp.Body.Close()

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		var respStruct ratingResponse
		err = json.Unmarshal(body, &respStruct)
		require.NoError(t, err)

		require.Equal(t, ratingResponse{
			AverageRating: 0.0,
			RatingCount:   0,
		}, respStruct)
	})

	t.Run("reviews -- adding review for the first time", func(t *testing.T) {
		payloadReader := strings.NewReader(`{
			"rating": 4,
			"comment": "Fantastic device"
		}`)
		req, err := http.NewRequestWithContext(t.Context(), "POST", baseUrl+"/api/reviews/reviewOrder/"+orderId, payloadReader)
		require.NoError(t, err)

		req.Header.Set("Content-Type", "application/json")
		req.Header.Set("Authorization", "Bearer "+renterToken)
		resp, err := http.DefaultClient.Do(req)
		require.NoError(t, err)
		require.Equal(t, http.StatusCreated, resp.StatusCode)
		resp.Body.Close()
	})

	t.Run("reviews -- adding review for the second time", func(t *testing.T) {
		payloadReader := strings.NewReader(`{
			"rating": 4,
			"comment": "Fantastic device"
		}`)
		req, err := http.NewRequestWithContext(t.Context(), "POST", baseUrl+"/api/reviews/reviewOrder/"+orderId, payloadReader)
		require.NoError(t, err)

		req.Header.Set("Content-Type", "application/json")
		req.Header.Set("Authorization", "Bearer "+renterToken)
		resp, err := http.DefaultClient.Do(req)
		require.NoError(t, err)
		require.Equal(t, http.StatusConflict, resp.StatusCode)
	})

	type review struct {
		ReviewId       int
		OrderId        string
		AuthorUsername string
		Rating         int
		Comment        string
		CreatedAt      time.Time
	}

	t.Run("reviews -- by device id", func(t *testing.T) {
		resp, err := http.Get(baseUrl + "/api/reviews/device/1")
		require.NoError(t, err)
		defer resp.Body.Close()
		require.Equal(t, http.StatusOK, resp.StatusCode)

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		var respStruct []review
		err = json.Unmarshal(body, &respStruct)
		require.NoError(t, err)

		require.Equal(t, 1, len(respStruct))
		expected := review{
			OrderId:        orderId,
			AuthorUsername: "TestRentingUser1",
			Rating:         4,
			Comment:        "Fantastic device",
		}

		require.Equal(t, expected.OrderId, respStruct[0].OrderId)
		require.Equal(t, expected.AuthorUsername, respStruct[0].AuthorUsername)
		require.Equal(t, expected.Rating, respStruct[0].Rating)
		require.Equal(t, expected.Comment, respStruct[0].Comment)
	})

	t.Run("reviews -- by username", func(t *testing.T) {
		resp, err := http.Get(baseUrl + "/api/reviews/user/TestOwner")
		require.NoError(t, err)
		defer resp.Body.Close()
		require.Equal(t, http.StatusOK, resp.StatusCode)

		body, err := io.ReadAll(resp.Body)
		require.NoError(t, err)

		var respStruct []review
		err = json.Unmarshal(body, &respStruct)
		require.NoError(t, err)

		require.Equal(t, 1, len(respStruct))
		expected := review{
			OrderId:        orderId,
			AuthorUsername: "TestRentingUser1",
			Rating:         4,
			Comment:        "Fantastic device",
		}

		require.Equal(t, expected.OrderId, respStruct[0].OrderId)
		require.Equal(t, expected.AuthorUsername, respStruct[0].AuthorUsername)
		require.Equal(t, expected.Rating, respStruct[0].Rating)
		require.Equal(t, expected.Comment, respStruct[0].Comment)
	})
}

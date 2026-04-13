package auth

import (
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/stretchr/testify/require"
)

func TestTokens(t *testing.T) {
	user := model.User{
		Name:     "testUser",
		Password: "testPassword",
		Admin:    false,
	}

	t.Run("non-admin -- should succeed", func(t *testing.T) {
		tokenBase, err := CreateToken(&user)
		require.NoError(t, err)
		require.NotEmpty(t, tokenBase)

		token, err := ParseToken(tokenBase)
		require.NoError(t, err)
		require.Equal(t, user.Name, token.Username)
		require.Equal(t, user.Admin, token.Admin)
	})

	t.Run("modified token (admin changed to true) -- should fail", func(t *testing.T) {
		tokenBase := "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6InRlc3RVc2VyIiwiYWRtaW4iOnRydWUsImV4cCI6MTc3NjA4MzkzNSwiaWF0IjoxNzc2MDgzMzM1fQ.QwYUtPog_WlB1-ryyyO_zXDEkcv7sR6NUg3JPKvCeCg"

		token, err := ParseToken(tokenBase)
		require.Error(t, err)
		require.Nil(t, token)
	})
}

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

	//t.Run("modified token -- should fail", func(t *testing.T) {
	//	tokenBase, err := CreateToken(&user)
	//	require.NoError(t, err)
	//	require.NotEmpty(t, tokenBase)
	//
	//	token, err := ParseToken(tokenBase)
	//	require.Error(t, err)
	//	require.Nil(t, token)
	//})
}

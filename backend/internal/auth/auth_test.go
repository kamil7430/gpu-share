package auth

import (
	"encoding/base64"
	"strings"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/ogen-go/ogen/json"
	"github.com/stretchr/testify/require"
)

type tokenStructure struct {
	Username string
	Admin    bool
	Exp      int64
	Iat      int64
}

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

	modifyTokenBody := func(token string, username *string, admin *bool) string {
		tokenFragments := strings.Split(token, ".")
		decoded := make([]byte, base64.RawURLEncoding.DecodedLen(len(tokenFragments[1])))
		_, err := base64.RawURLEncoding.Decode(decoded, []byte(tokenFragments[1]))
		require.NoError(t, err)
		var tokenObj tokenStructure
		err = json.Unmarshal(decoded, &tokenObj)
		require.NoError(t, err)

		if username != nil {
			tokenObj.Username = *username
		}
		if admin != nil {
			tokenObj.Admin = *admin
		}

		modifiedToken, err := json.Marshal(tokenObj)
		require.NoError(t, err)
		encoded := make([]byte, base64.RawURLEncoding.EncodedLen(len(modifiedToken)))
		base64.RawURLEncoding.Encode(encoded, modifiedToken)
		tokenFragments[1] = string(encoded)
		return strings.Join(tokenFragments, ".")
	}

	t.Run("modified token (admin changed to true) -- should fail", func(t *testing.T) {
		validToken, err := CreateToken(&user)
		require.NoError(t, err)

		admin := true
		modifiedToken := modifyTokenBody(validToken, nil, &admin)

		token, err := ParseToken(modifiedToken)
		require.Error(t, err)
		require.Nil(t, token)
	})

	t.Run("modified token (username changed) -- should fail", func(t *testing.T) {
		validToken, err := CreateToken(&user)
		require.NoError(t, err)

		username := "InnyUser"
		modifiedToken := modifyTokenBody(validToken, &username, nil)

		token, err := ParseToken(modifiedToken)
		require.Error(t, err)
		require.Nil(t, token)
	})
}

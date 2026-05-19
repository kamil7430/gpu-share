package cli

import (
	"os"
)

const tokenFile = ".agent_token"

func SaveToken(token string) error {
	return os.WriteFile(tokenFile, []byte(token), 0600)
}

func LoadToken() (string, error) {
	data, err := os.ReadFile(tokenFile)
	if err != nil {
		return "", err
	}
	return string(data), nil
}

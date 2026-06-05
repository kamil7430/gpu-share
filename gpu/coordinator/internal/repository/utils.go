package repository

import "os"

const (
	backendPort = "2137"
)

func backendIp() string {
	backIp := os.Getenv("BACKEND_IP")
	if backIp == "" {
		backIp = "127.0.0.1"
	}
	return backIp
}

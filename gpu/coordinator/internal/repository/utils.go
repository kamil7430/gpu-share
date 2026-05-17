package repository

import "os"

const (
	backendPort = "2137"
)

func backendIp() string {
	backIp := os.Getenv("BACKEND_IP")
	if backIp == "" {
		backIp = "10.5.0.2"
	}
	return backIp
}

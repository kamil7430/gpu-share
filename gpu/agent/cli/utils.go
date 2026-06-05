package cli

import (
	"bufio"
	"fmt"
	"log"
	"os"
	"strconv"
	"strings"
)

const (
	backendPort = "2137"
	coordinatorGrpcPort = "2139"
)

func backendIp() string {
	backIp := os.Getenv("BACKEND_IP")
	if backIp == "" {
		backIp = "127.0.0.1"
	}
	return backIp
}

func gpuIp() string {
	gpuIp := os.Getenv("GPU_IP")
	if gpuIp == "" {
		gpuIp = "127.0.0.1"
	}
	return gpuIp
}

func promptString(reader *bufio.Reader, label string) string {
	fmt.Printf("%s: ", label)

	value, err := reader.ReadString('\n')
	if err != nil {
		log.Fatal(err)
	}

	return strings.TrimSpace(value)
}

func promptInt(reader *bufio.Reader, label string) int {
	for {
		value := promptString(reader, label)

		n, err := strconv.Atoi(value)
		if err != nil {
			fmt.Println("enter a valid integer")
			continue
		}

		return n
	}
}

func promptIntWithDefault(reader *bufio.Reader, def int, label string) int {
	for {
		value := promptString(reader, fmt.Sprintf("%v (%v)", label, def))
		if value == "" {
			return def
		}

		n, err := strconv.Atoi(value)
		if err != nil {
			fmt.Println("enter a valid integer")
			continue
		}

		return n
	}
}

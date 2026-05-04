package agent

import (
	"bytes"
	"context"
	"encoding/json"
	"flag"
	"fmt"
	"net/http"
	"os"
)

func ConnectCmd(args []string) {
	fs := flag.NewFlagSet("connect", flag.ExitOnError)
	deviceID := fs.String("device", "", "device id")

	backIp := os.Getenv("BACKEND_IP")
	if backIp == "" {
		backIp = "10.5.0.2"
	}
	backend := fs.String("backend", backIp+":2137", "backend addr")
	gpuIp := os.Getenv("GPU_IP")
	if gpuIp == "" {
		gpuIp = "10.5.0.3"
	}
	coord := fs.String("coord", gpuIp+":2139", "coordinator addr")
	fs.Parse(args)

	token, err := LoadToken()
	if err != nil {
		panic("not logged in")
	}

	req, _ := http.NewRequest(
		"POST",
		*backend+"/api/devices/"+*deviceID+"/connect",
		bytes.NewBuffer([]byte("{}")),
	)
	req.Header.Set("Authorization", "Bearer "+token)

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()

	var result struct {
		Token string `json:"token"`
	}
	json.NewDecoder(resp.Body).Decode(&result)

	stream, err := StartGrpcClient(context.Background(), *coord)
	if err != nil {
		panic(err)
	}

	err = SendHelloMessage(stream, *deviceID, result.Token)
	if err != nil {
		panic(err)
	}

	fmt.Println("connected as device:", *deviceID)

	go SendHeartbeats(context.Background(), stream, *deviceID)
	ReceiveLoop(context.Background(), stream, *deviceID)
}

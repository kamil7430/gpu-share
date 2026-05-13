package cli

import (
	"bufio"
	"bytes"
	"context"
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"net/http"
	"os"

	"github.com/kamil7430/gpu-share/gpu/agent/agent"
)

func ConnectCmd(args []string) {
	fs := flag.NewFlagSet("connect", flag.ExitOnError)
	deviceID := fs.String("device", "", "device ID")

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

	reader := bufio.NewReader(os.Stdin)

	if *deviceID == "" {
		devices, err := ListDevices()
		if err != nil {
			log.Fatal(err)
		}

		deviceIndex := 0
		for {
			deviceIndex = promptInt(reader, "device index")
			if !(deviceIndex >= 0 && deviceIndex < len(devices)) {
				fmt.Printf("index must be in [0, %v)\n", len(devices))
				continue
			}
			break
		}

		*deviceID = devices[deviceIndex].DeviceID
	}

	url := "http://" + *backend + "/api/devices/"
	req, err := http.NewRequest(
		"POST",
		url+*deviceID,
		bytes.NewBuffer([]byte("{}")),
	)
	if err != nil {
		log.Fatal(err)
	}
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

	stream, err := agent.StartGrpcClient(context.Background(), *coord)
	if err != nil {
		log.Fatal(err)
	}

	if err = agent.SendHelloMessage(stream, *deviceID, result.Token); err != nil {
		log.Fatal(err)
	}

	fmt.Printf("connected as device with id %v\n", *deviceID)

	go agent.SendHeartbeats(context.Background(), stream, *deviceID)

	if err = agent.ReceiveLoop(context.Background(), stream, *deviceID); err != nil {
		log.Fatal(err)
	}
}

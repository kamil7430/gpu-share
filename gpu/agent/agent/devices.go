package agent

import (
	"encoding/json"
	"flag"
	"fmt"
	"net/http"
	"os"
)

func DevicesCmd(args []string) {
	if len(args) < 1 {
		fmt.Println("expected 'list'")
		return
	}

	switch args[0] {
	case "list":
		ListDevices()
	default:
		fmt.Println("unknown devices subcommand")
	}
}

func ListDevices() {
	token, err := LoadToken()
	if err != nil {
		panic("not logged in")
	}

	fs := flag.NewFlagSet("devices", flag.ExitOnError)

	backIp := os.Getenv("BACKEND_IP")
	if backIp == "" {
		backIp = "10.5.0.2"
	}
	backend := fs.String("backend", backIp+":2137", "backend addr")
	req, _ := http.NewRequest("GET", "http://"+*backend+"/api/devices", nil)
	req.Header.Set("Authorization", "Bearer "+token)

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()

	var devices []map[string]any
	json.NewDecoder(resp.Body).Decode(&devices)

	for _, d := range devices {
		fmt.Printf("ID: %v, Name: %v\n", d["deviceId"], d["name"])
	}
}

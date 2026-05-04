package agent

import (
	"encoding/json"
	"fmt"
	"net/http"
)

func devicesCmd(args []string) {
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

	req, _ := http.NewRequest("GET", "http://localhost:8080/api/devices", nil)
	req.Header.Set("Authorization", "Bearer "+token)

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()

	var devices []map[string]interface{}
	json.NewDecoder(resp.Body).Decode(&devices)

	for _, d := range devices {
		fmt.Printf("ID: %v, Name: %v\n", d["deviceId"], d["name"])
	}
}

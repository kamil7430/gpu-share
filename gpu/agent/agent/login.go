package agent

import (
	"bytes"
	"encoding/json"
	"flag"
	"fmt"
	"net/http"
	"os"
)

func LoginCmd(args []string) {
	fs := flag.NewFlagSet("login", flag.ExitOnError)
	username := fs.String("u", "", "username")
	password := fs.String("p", "", "password")
	envIp := os.Getenv("BACKEND_IP")
	if envIp == "" {
		envIp = "10.5.0.2"
	}
	addr := flag.String("addr", envIp+":2137", "backend addr")
	flag.Parse()

	if *username == "" {
		fmt.Print("username> ")
		fmt.Scan(username)
	}
	if *password == "" {
		fmt.Print("password> ")
		fmt.Scan(password)
	}

	body, _ := json.Marshal(map[string]string{
		"username": *username,
		"password": *password,
	})

	resp, err := http.Post("http://"+*addr+"/api/users/login", "application/json", bytes.NewBuffer(body))
	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()

	var result struct {
		Token string `json:"token"`
	}

	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		panic(err)
	}

	if err := SaveToken(result.Token); err != nil {
		panic(err)
	}

	fmt.Println("logged in")
}

package cli

import (
	"bufio"
	"bytes"
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"net/http"
	"os"
)

func LoginCmd(args []string) {
	fs := flag.NewFlagSet("login", flag.ContinueOnError)
	username := fs.String("u", "", "username")
	password := fs.String("p", "", "password")
	addr := fs.String("addr", backendIp()+":"+backendPort, "backend addr")
	fs.Parse(args)

	reader := bufio.NewReader(os.Stdin)

	if *username == "" {
		*username = promptString(reader, "username")
	}
	if *password == "" {
		*password = promptString(reader, "password")
	}

	body, _ := json.Marshal(map[string]string{
		"username": *username,
		"password": *password,
	})

	resp, err := http.Post("http://"+*addr+"/api/users/login", "application/json", bytes.NewBuffer(body))
	if err != nil {
		log.Fatalf("couldn't connect to %v (%v)", *addr, err)
	}
	if resp.StatusCode == 401 {
		log.Fatal("incorrect password")
	}
	if resp.StatusCode == 404 {
		log.Fatal("username not found")
	}
	if resp.StatusCode != 200 {
		log.Fatal("server error")
	}
	defer resp.Body.Close()

	var result struct {
		Token string `json:"token"`
	}

	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		log.Fatal(err)
	}

	if err := SaveToken(result.Token); err != nil {
		log.Fatal(err)
	}

	fmt.Println("logged in")
}

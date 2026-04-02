package main

import (
	"errors"
	"log"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/cmd/server"
	"github.com/kamil7430/gpu-share/backend/internal"
)

func fatalIfError(err error) {
	if err != nil {
		log.Fatal(err)
	}
}

func main() {
	log.Println("Starting server...")
	db, err := internal.InitializeDatabaseConnection(true)
	fatalIfError(err)

	log.Println("Building server instance...")
	srv := server.NewServer(db)

	log.Println("Started server!")
	err = http.ListenAndServe(":2137", srv)
	if !errors.Is(err, http.ErrServerClosed) {
		log.Fatal(err)
	}
}

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
	server := server.NewServer(db)
	defer fatalIfError(server.Close())

	log.Println("Started server!")
	err = server.ListenAndServe()
	if !errors.Is(err, http.ErrServerClosed) {
		log.Fatal(err)
	}
}

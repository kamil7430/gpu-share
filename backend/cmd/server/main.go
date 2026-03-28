package main

import (
	"log"
	"net/http"

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
	mux := NewServer(db)

	log.Println("Started server!")
	log.Fatal(http.ListenAndServe(":2137", mux))
}

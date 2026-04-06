package main

import (
	"errors"
	"log"
	"net/http"

	"github.com/kamil7430/gpu-share/backend/cmd/server"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
)

func fatalIfError(err error) {
	if err != nil {
		log.Fatal(err)
	}
}

func main() {
	log.Println("Starting server...")
	db, err := utils.InitializeDatabaseConnection(true)
	fatalIfError(err)

	log.Println("Building server instance...")
	srv := server.NewServer(db)
	defer func() {
		err := srv.Close()
		if err != nil {
			log.Println(err)
		}
	}()

	log.Println("Started server!")
	err = srv.ListenAndServe()
	if !errors.Is(err, http.ErrServerClosed) {
		log.Fatal(err)
	}
}

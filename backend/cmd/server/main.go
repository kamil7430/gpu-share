package main

import (
	"fmt"
	"log"
	"net/http"
	"os"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
)

func fatalIfError(err error) {
	if err != nil {
		log.Fatal(err)
	}
}

func main() {
	dbUser := os.Getenv("POSTGRES_USER")
	dbPassword := os.Getenv("POSTGRES_PASSWORD")
	dbDb := os.Getenv("POSTGRES_DB")
	dbPort := os.Getenv("POSTGRES_DB_PORT")

	dsn := fmt.Sprintf("host=db user=%s password=%s dbname=%s port=%s sslmode=disable",
		dbUser, dbPassword, dbDb, dbPort)
	db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{
		TranslateError: true,
	})
	fatalIfError(err)

	err = db.AutoMigrate(&model.Device{})
	fatalIfError(err)

	mux := NewServer(db)

	log.Println("Started server...")
	log.Fatal(http.ListenAndServe(":2137", mux))
}

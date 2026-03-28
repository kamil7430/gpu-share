package internal

import (
	"fmt"
	"log"
	"os"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
)

func InitializeDatabaseConnection(verbose bool) (*gorm.DB, error) {
	if verbose {
		log.Println("Loading environment variables...")
	}
	dbUser := os.Getenv("POSTGRES_USER")
	dbPassword := os.Getenv("POSTGRES_PASSWORD")
	dbDb := os.Getenv("POSTGRES_DB")
	dbPort := os.Getenv("POSTGRES_DB_PORT")

	if verbose {
		log.Println("Connecting to the database...")
	}
	dsn := fmt.Sprintf("host=db user=%s password=%s dbname=%s port=%s sslmode=disable",
		dbUser, dbPassword, dbDb, dbPort)
	db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{
		TranslateError: true,
	})
	if err != nil {
		return nil, err
	}

    if verbose {
	    log.Println("Migrating models...")
    }
	err = db.AutoMigrate(&model.Device{})
	if err != nil {
		return nil, err
	}

	return db, nil
}

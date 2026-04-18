package utils

import (
	"fmt"
	"log"
	"os"
	"sync"

	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
)

var migratedMutex sync.Mutex
var migrated bool = false

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

	migratedMutex.Lock()
	if !migrated {
		migrated = true
		if verbose {
			log.Println("Migrating models...")
		}
		err = db.AutoMigrate(&model.Device{}, &model.User{})
		if err != nil {
			return nil, err
		}
	}
	migratedMutex.Unlock()

	return db, nil
}

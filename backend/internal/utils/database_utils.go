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

func performMigration(db *gorm.DB) error {
	// Migration should be performed in a specific order. Since the database traces the dependencies
	// (foreign keys consistency) between the tables, we have to migrate the "leaves" of the dependency
	// tree first. In other words, if the dependency tree looks like this:
	//     C <- B1 <- A -> B2,
	// we should migrate C before B1, and B1 together with B2 before A.
	return db.AutoMigrate(&model.User{}, &model.Device{})
}

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
		err = performMigration(db)
		if err != nil {
			return nil, err
		}
	}
	migratedMutex.Unlock()

	return db, nil
}

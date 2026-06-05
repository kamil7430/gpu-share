package utils

import (
	"encoding/csv"
	"fmt"
	"log"
	"math/rand"
	"os"
	"strconv"
	"sync"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"golang.org/x/crypto/bcrypt"
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
	return db.AutoMigrate(
		&model.User{},
		&model.Device{},
		&model.Order{},
	)
}

func InitializeDatabaseConnection(verbose bool) (*gorm.DB, error) {
	if verbose {
		log.Println("Loading environment variables...")
	}
	dbUser := GetenvOrDefault("POSTGRES_USER", "user")
	dbPassword := GetenvOrDefault("POSTGRES_PASSWORD", "pass")
	dbDb := GetenvOrDefault("POSTGRES_DB", "db")
	dbPort := GetenvOrDefault("POSTGRES_DB_PORT", "5432")

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

func IsDatabaseEmpty(db *gorm.DB) bool {
	var count int64
	db.Model(&model.Device{}).Count(&count)
	return count == 0
}

func SeedDatabase(db *gorm.DB) error {
	users, err := createUsers(db, 10)
	if err != nil {
		return err
	}

	return loadDevicesFromCSV(db, users, "data/gpus.csv")
}

func createUsers(db *gorm.DB, count int) ([]model.User, error) {
	users := make([]model.User, 0, count)

	hashed, err := bcrypt.GenerateFromPassword(
		[]byte("password"),
		bcrypt.DefaultCost,
	)
	if err != nil {
		panic("unreachable")
	}

	for i := range count {
		user := model.User{
			Name:               fmt.Sprintf("user%d", i+1),
			Password:           string(hashed),
			WalletBalanceCents: rand.Intn(100000),
		}

		if err := db.Create(&user).Error; err != nil {
			return nil, err
		}

		users = append(users, user)
	}

	return users, nil
}

func loadDevicesFromCSV(
	db *gorm.DB,
	users []model.User,
	path string,
) error {
	f, err := os.Open(path)
	if err != nil {
		return err
	}
	defer f.Close()

	reader := csv.NewReader(f)

	rows, err := reader.ReadAll()
	if err != nil {
		return err
	}

	for i, row := range rows {
		if i == 0 {
			continue // header
		}

		vram, _ := strconv.Atoi(row[2])
		cudaCores, _ := strconv.Atoi(row[3])
		price, _ := strconv.Atoi(row[4])
		major, _ := strconv.Atoi(row[5])
		minor, _ := strconv.Atoi(row[6])

		owner := users[rand.Intn(len(users))]

		device := model.Device{
			Name:                 row[0],
			GpuModel:             row[1],
			VramMb:               vram,
			CudaCores:            cudaCores,
			PricePerHourUsdCents: price,
			DriverVersionMajor:   major,
			DriverVersionMinor:   minor,
			State:                api.StateAVAILABLE,
			UserID:               owner.ID,
		}

		if err := db.Create(&device).Error; err != nil {
			return err
		}
	}

	return nil
}

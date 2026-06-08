package seeder

import (
	"encoding/csv"
	"fmt"
	"math/rand"
	"os"
	"strconv"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/auth"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"gorm.io/gorm"
)

func SeedDatabase(db *gorm.DB) error {
	users, err := createUsers(db, 10)
	if err != nil {
		return err
	}

	return loadDevicesFromCSV(db, users, "device_seed.csv")
}

func createUsers(db *gorm.DB, count int) ([]model.User, error) {
	users := make([]model.User, 0, count)

	hashed, err := auth.HashPassword("password")
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

		cudaCores, _ := strconv.Atoi(row[2])
		vram, _ := strconv.Atoi(row[3])
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


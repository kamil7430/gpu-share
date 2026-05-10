package repository

import (
	"errors"
	"strconv"
	"testing"

	"github.com/kamil7430/gpu-share/backend/internal/api"
	"github.com/kamil7430/gpu-share/backend/internal/model"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
	"github.com/stretchr/testify/require"
)

func TestTransaction(t *testing.T) {
	db, err := utils.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	dr := NewDeviceRepository(tx)

	resetDbContent := func() {
		tx.Exec("TRUNCATE TABLE devices, users;")
		tx.Exec("INSERT INTO users(id, name, password, admin) VALUES ('1', 'TestUser', 'DISABLED', 'false');")
	}

	checkDeviceExist := func(dr DeviceRepository, id uint) {
		device, err := dr.GetDeviceById(t.Context(), strconv.Itoa(int(id)))
		require.NoError(t, err)
		require.NotNil(t, device)
		require.Equal(t, "TestCard", device.Name)
		require.Equal(t, "NVIDIA GeForce RTX 3050", device.GpuModel)
		require.Equal(t, 8192, device.VramMb)
		require.Equal(t, 2560, device.CudaCores)
		require.Equal(t, 1599, device.PricePerHourUsdCents)
		require.Equal(t, 595, device.DriverVersionMajor)
		require.Equal(t, 97, device.DriverVersionMinor)
		require.Equal(t, api.StateUNAVAILABLE, device.State)
		require.Equal(t, uint(1), device.UserID)
	}

	t.Run("transaction commit", func(t *testing.T) {
		resetDbContent()

		tr := Transaction{
			Dr: dr,
		}

		var devId uint

		err := tr.WithTransaction(func(tran *Transaction) error {
			dev := &model.Device{
				Name:                 "TestCard",
				GpuModel:             "NVIDIA GeForce RTX 3050",
				VramMb:               8192,
				CudaCores:            2560,
				PricePerHourUsdCents: 1599,
				DriverVersionMajor:   595,
				DriverVersionMinor:   97,
				State:                api.StateUNAVAILABLE,
				UserID:               1,
			}

			err := tran.Dr.AddDevice(t.Context(), dev)
			require.NoError(t, err)

			devId = dev.ID
			checkDeviceExist(tran.Dr, devId)

			return nil
		})
		require.NoError(t, err)

		checkDeviceExist(dr, devId)
	})

	t.Run("transaction rollback", func(t *testing.T) {
		resetDbContent()

		tr := Transaction{
			Dr: dr,
		}

		var devId uint

		err := tr.WithTransaction(func(tran *Transaction) error {
			dev := &model.Device{
				Name:                 "TestCard",
				GpuModel:             "NVIDIA GeForce RTX 3050",
				VramMb:               8192,
				CudaCores:            2560,
				PricePerHourUsdCents: 1599,
				DriverVersionMajor:   595,
				DriverVersionMinor:   97,
				State:                api.StateUNAVAILABLE,
				UserID:               1,
			}

			err := tran.Dr.AddDevice(t.Context(), dev)
			require.NoError(t, err)

			devId = dev.ID
			checkDeviceExist(tran.Dr, devId)

			return errors.New("test error") // should rollback
		})
		require.Error(t, err)

		device, err := dr.GetDeviceById(t.Context(), strconv.Itoa(int(devId)))
		require.Error(t, err)
		require.Nil(t, device)
	})
}

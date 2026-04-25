package api_tests

import (
	"errors"
	"log"
	"net/http"
	"os"
	"testing"
	"time"

	"github.com/kamil7430/gpu-share/backend/cmd/server"
	"github.com/kamil7430/gpu-share/backend/internal/repository"
	"github.com/kamil7430/gpu-share/backend/internal/utils"
	"github.com/stretchr/testify/require"
	"gorm.io/gorm"
)

var baseUrl = func() string {
	ip := os.Getenv("BACKEND_IP")
	if ip == "" {
		log.Fatal("invalid value of `BACKEND_IP` env variable")
	}
	return "http://" + ip + ":2137"
}()

var testsToRun = []func(*testing.T, *gorm.DB, string){
	testGetDeviceStatus,
	testGetDevices,
	testAddDevice,
	testLogin,
	testRegister,
	testChangePassword,
	testOrderDevice,
}

func TestApi(t *testing.T) {
	db, err := utils.InitializeDatabaseConnection(false)
	require.NoError(t, err)

	tx := db.Begin()
	defer tx.Rollback()

	repos := server.Repos{
		DeviceRepo: repository.NewDeviceRepository(tx),
		GpuRepo:    repository.NewMockGpuRepository(),
		UserRepo:   repository.NewUserRepository(tx),
	}

	srv := server.NewServer(&repos)
	defer func() {
		err := srv.Shutdown(t.Context())
		if err != nil {
			log.Println(err)
		}
	}()
	go func() {
		err := srv.ListenAndServe()
		if !errors.Is(err, http.ErrServerClosed) {
			log.Fatal(err)
		}
	}()

	log.Println("Checking whether server is up...")
	retries := 10
	i := 0
	for ; i < retries; i += 1 {
		resp, err := http.Get(baseUrl + "/health")
		if err == nil && resp.StatusCode == 200 {
			break
		}
		log.Printf("Failed, retrying in one second... (try no.: %v/%v)\n", i+1, retries)
		time.Sleep(time.Second)
	}
	require.Less(t, i, retries, "Could not connect to server!")
	log.Println("Server is up! Running the tests...")

	for _, test := range testsToRun {
		test(t, tx, baseUrl)
	}
}

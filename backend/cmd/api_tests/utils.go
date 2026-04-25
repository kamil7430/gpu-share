package api_tests

import "gorm.io/gorm"

func truncateTables(db *gorm.DB) {
	db.Exec("TRUNCATE TABLE devices, users, orders;")
}

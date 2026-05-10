package repository

import (
	"log"

	"gorm.io/gorm"
)

// Transaction struct is intended to aggregate repositories that use gorm to communicate with
// the database. Because of this program's design decisions, the need for transactions causes the leak
// of abstraction.
//
// To use this wrapper, spawn a new instance of Transaction struct (populating only needed fields) and
// use the WithTransaction method to begin a transaction.
type Transaction struct {
	// remember to update the WithTransaction method (the `return fn(...)` statement)
	// and retrieveDb method (add new else) every time this repos list changes
	Dr DeviceRepository
	Ur UserRepository
}

// WithTransaction starts a transaction as a block. Returning an error will rollback, otherwise will commit.
// Please note that usage of nil-initialized fields of Transaction struct within the transaction
// will cause the program panic.
func (t *Transaction) WithTransaction(fn func(transaction *Transaction) error) error {
	db := t.retrieveDb()

	return db.Transaction(func(tx *gorm.DB) error {
		return fn(&Transaction{
			Dr: t.Dr,
			Ur: t.Ur,
			// update on every repo list change
		})
	})
}

func (t *Transaction) retrieveDb() *gorm.DB {
	var db *gorm.DB

	if t.Dr != nil {
		db = t.Dr.(*deviceRepository).db
	} else if t.Ur != nil {
		db = t.Ur.(*userRepository).db
	}
	// update the logic on every repo list change

	if db == nil {
		log.Fatal("Couldn't retrieve db pointer for transaction")
	}

	return db
}

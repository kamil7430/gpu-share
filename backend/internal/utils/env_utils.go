package utils

import (
	"log"
	"os"
)

func GetenvOrDefault(key string, def string) string {
	val := os.Getenv(key)
	if val == "" {
		val = def
		log.Printf("%v not present or empty, using default value '%v'\n", key, val)
	}
	return val
}

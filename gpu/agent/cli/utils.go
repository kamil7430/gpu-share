package cli

import (
	"bufio"
	"fmt"
	"log"
	"strconv"
	"strings"
)

func promptString(reader *bufio.Reader, label string) string {
	fmt.Printf("%s: ", label)

	value, err := reader.ReadString('\n')
	if err != nil {
		log.Fatal(err)
	}

	return strings.TrimSpace(value)
}

func promptInt(reader *bufio.Reader, label string) int {
	for {
		value := promptString(reader, label)

		n, err := strconv.Atoi(value)
		if err != nil {
			fmt.Println("enter a valid integer")
			continue
		}

		return n
	}
}

func promptIntWithDefault(reader *bufio.Reader, def int, label string) int {
	for {
		value := promptString(reader, fmt.Sprintf("%v (%v)", label, def))
		if value == "" {
			return def
		}

		n, err := strconv.Atoi(value)
		if err != nil {
			fmt.Println("enter a valid integer")
			continue
		}

		return n
	}
}

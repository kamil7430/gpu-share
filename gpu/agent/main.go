package main

import (
	"fmt"
	"os"

	"github.com/kamil7430/gpu-share/gpu/agent/cli"
)

func usage() {
	fmt.Println("usage: agent [login|devices|connect]")
	os.Exit(1)
}

var modes = []string{"login", "devices", "connect"}

func chooseAction() int {
	for i, m := range modes {
		fmt.Printf("%v) %v\n", i+1, m)
	}

	for {
		fmt.Print("Choose action> ")
		var i int
		if n, err := fmt.Scan(&i); err != nil || n != 1 {
			continue
		}
		if i > 0 && i <= len(modes) {
			return i - 1
		}
	}
}

func main() {
	var cmd string
	args := os.Args
	if len(args) < 2 {
		cmd = modes[chooseAction()]
	} else {
		cmd = os.Args[1]
		args = args[2:]
	}

	switch cmd {
	case "login":
		cli.LoginCmd(args)
	case "devices":
		cli.ListDevices()
	case "connect":
		cli.ConnectCmd(args)
	default:
		usage()
	}
}

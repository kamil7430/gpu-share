package main

import (
	"fmt"
	"os"
	
	"github.com/kamil7430/gpu-share/gpu/agent/agent"
)

func usage() {
	fmt.Println("usage: agent [login|devices|connect]")
	os.Exit(1)
}

var modes = []string{"login", "devices", "connect"}

func chooseMode() int {
	for i, m := range modes {
		fmt.Printf("%v) %v\n", i+1, m)
	}

	for {
		fmt.Print("Choose mode> ")
		var i int
		fmt.Scan(&i)
		if i > 0 && i <= len(modes) {
			return i - 1
		}
	}
}

func main() {
	var cmd string
	args := os.Args
	if len(args) < 2 {
		cmd = modes[chooseMode()]
	} else {
		cmd = os.Args[1]
		args = args[2:]
	}

	switch cmd {
	case "login":
		agent.LoginCmd(args)
	case "devices":
		agent.ListDevices()
	case "connect":
		agent.ConnectCmd(args)
	default:
		usage()
	}
}

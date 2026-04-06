#!/bin/bash

source ./backend/.env.example

cd backend
go test -p 1 ./... -v -count=1

cd ../gpu
go test -p 1 ./... -v -count=1

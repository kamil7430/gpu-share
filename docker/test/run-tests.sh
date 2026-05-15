#!/bin/bash

set -e # stop on failure

source ./.env

# backend
cd backend
go test -p 1 ./... -count=1
backend_exit=$?

# gpu
go run cmd/main.go &
cd ../gpu
go test -p 1 ./... -count=1
gpu_exit=$?

if [ $backend_exit -ne 0 ]; then
  exit $backend_exit
else
  exit $gpu_exit
fi

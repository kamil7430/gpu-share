#!/bin/bash

source ./.env.example

cd backend
go test -p 1 ./... -v -count=1
backend_exit=$?

cd ../gpu
go test -p 1 ./... -v -count=1
gpu_exit=$?

if [ $backend_exit -ne 0 ]; then
  exit $backend_exit
else
  exit $gpu_exit
fi

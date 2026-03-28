#!/bin/bash

source .env.example

go test -p 1 ./... -v


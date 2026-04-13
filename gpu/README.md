## Usage

```bash
# run the backend and coordinator through docker
cd docker && docker compose up --build
```

Run the agent. If you don't provide an ip and have a valid `.env` file, it will
use the value of the `GPU_IP` variable.
```bash
go run agent/main.go [--ip] [--port]
```

## Building protobuf

```bash
go install google.golang.org/protobuf/cmd/protoc-gen-go@latest
go install google.golang.org/grpc/cmd/protoc-gen-go-grpc@latest
```

```bash
./proto-compile.sh
```

This will generate `./proto/*.pb.go`.

## Useful links

- https://grpc.io/docs/languages/go/basics/
- https://github.com/grpc/grpc-go/tree/master/examples/route_guide
- https://protobuf.dev/programming-guides/proto3/

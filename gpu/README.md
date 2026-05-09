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

You might want to register a test account and register a device:
```bash
curl -v -X POST http://localhost:2137/api/users/register -H "Content-Type: application/json" -d '{"username": "test", "password": "maklowicz"}'

curl -v -X POST http://localhost:2137/api/devices -H "Content-Type: application/json" -H "Authorization: Bearer $(cat .agent_token)" -d '{"name":"GPU_Maklowicza1","gpuModel":"RTX 5090","vramMb":32000,"cudaCores":21760,"pricePerHourUsdCents":2000, "driverVersion": "596.36"}'
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

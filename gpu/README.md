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

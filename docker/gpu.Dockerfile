FROM golang:1.26.1

WORKDIR /app

COPY gpu/go.mod gpu/go.sum .
RUN go mod download -x

COPY gpu ./gpu

WORKDIR /app/gpu/coordinator
RUN go build -o server ./cmd

CMD ["./server"]

FROM golang:1.26.1

WORKDIR /app

COPY backend/go.mod backend/go.sum ./
RUN go mod download

COPY backend ./backend
COPY frontend ./frontend

WORKDIR /app/backend
RUN go build -o server ./cmd

CMD ["./server"]

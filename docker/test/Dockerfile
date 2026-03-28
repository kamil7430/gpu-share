FROM golang:1.26.1

WORKDIR /app

COPY backend/go.mod backend/go.sum ./
RUN go mod download

COPY backend ./backend
COPY frontend ./frontend
COPY docker/test/run-tests.sh ./backend
COPY backend/.env.example ./backend/.env

WORKDIR /app/backend

RUN chmod +x run-tests.sh

ENTRYPOINT ["./run-tests.sh"]


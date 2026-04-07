FROM golang:1.26.1

WORKDIR /app/backend
COPY backend/go.mod backend/go.sum .
RUN go mod download -x

WORKDIR /app/gpu
COPY gpu/go.mod gpu/go.sum .
RUN go mod download -x

WORKDIR /app
COPY .env.example .
COPY backend ./backend
COPY gpu ./gpu
COPY frontend ./frontend
COPY docker/test/run-tests.sh .

RUN chmod +x run-tests.sh

CMD ["./run-tests.sh"]

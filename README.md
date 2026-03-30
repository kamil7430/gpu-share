# Gpu Share

**Gpu Share** is a platform to rent gpu compute. It allows gpu owners to offer their gpu compute to others. Users can borrow gpu compute without need to buy gpu themselves.

## Requirements

- `Docker`
- `Docker Compose`

## Usage

```bash
git clone https://github.com/kamil7430/gpu-share.git
```

Make sure that Docker daemon is up.

```bash
sudo systemctl start dockerd
```

### Server

```bash
cp backend/.env.example backend/.env # Change .env if you wish
cd docker
docker compose up --build
```

### Tests

```bash
cd docker/test
docker compose up --build --abort-on-container-exit
```

## Tech stack

### Backend

- `Golang`
- `GORM`
- `REST API`
- `JWT`
- `PostgreSQL`
- `JSON Schema`

### Frontend

- `Blazor` (expected)

### Others

- `Docker`
- `GitHub Actions`
- `GitHub Issues`

## Project structure

```text
backend/
  cmd/
  internal/
docs/
  use-cases/       scenariusze funkcjonalne
  architecture/    opis struktury systemu
  domain/          model domenowy
  decisions/       ADR-y
frontend/
  # TODO
```

## Recommended workflow

1. Describe the use case and add an Issue.
2. Describe important decisions in the ADR directory.
3. Create a new feature branch.
4. Add unit and integration tests.
5. Develop production code.
6. When the feature is ready, create a pull request.

## Authors

- Kamil Błażejczyk ([kamil7430](https://github.com/kamil7430))
- Paweł Bielecki ([FreePlacki](https://github.com/FreePlacki))
- Kacper Grobel ([MajorFallen](https://github.com/MajorFallen))
- Anna Babicka ([ababicka11011](https://github.com/ababicka11011))
- Marcin Sulecki ([sulmar](https://github.com/sulmar))

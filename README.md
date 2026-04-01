# Gpu Share

**Gpu Share** is a platform for renting gpu compute. It allows gpu owners to
offer their resources to others. Users can borrow gpu compute without the need
to buy a gpu themselves.

## Requirements

- `Docker`
- `Docker Compose`

## Usage

```bash
git clone https://github.com/kamil7430/gpu-share.git
```

Make sure that Docker daemon is up.

```bash
sudo systemctl start docker
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
- `OpenAPI`

### Frontend

- `Blazor` (expected)

### Others

- `Docker`
- `GitHub Actions`
- `GitHub Issues`

## Project structure

```text
backend/
  cmd/                       command-line-functional libs
  internal/                  internal libraries
  tools/                     codegen tools
contract/
  openapi/                   openapi contracts
docs/
  decisions/                 ADRs
  project_documentation.pdf  specification
  # user stories in Issues
frontend/                    future project structure for Blazor
```

## Recommended workflow

1. Describe the use case and add an Issue.
2. Describe important decisions in the ADR directory.
3. Create a new feature branch.
4. If changing the API, first change the openapi yaml specification and run
codegen using `cd backend && go generate ./...`
5. Add unit and integration tests.
6. Develop production code.
7. When the feature is ready, create a pull request.

## Authors

- Kamil Błażejczyk ([kamil7430](https://github.com/kamil7430))
- Paweł Bielecki ([FreePlacki](https://github.com/FreePlacki))
- Kacper Grobel ([MajorFallen](https://github.com/MajorFallen))
- Anna Babicka ([ababicka11011](https://github.com/ababicka11011))
- Marcin Sulecki ([sulmar](https://github.com/sulmar))

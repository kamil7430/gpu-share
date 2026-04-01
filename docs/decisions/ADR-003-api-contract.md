## Status

Decyzja: OpenAPI

## Kontekst

Rozważamy formalne zapisanie kontraktu API. Opcje rozważane: JSON Schema (ogólna walidacja danych) oraz OpenAPI (specyfikacja endpointów, metod, parametrów, schematów). Potrzebujemy czytelnej, narzędziowo wspieranej specyfikacji dla deweloperów, dokumentacji i generowania klienta/serwera.

Do wyboru standardy:
- JSON schema
- OpenAPI

## Decyzja

Wybieramy OpenAPI jako oficjalny format kontraktu API.

- Pokrywa zarówno definicję schematów (JSON Schema-like) jak i kompletne API (ścieżki, metody, parametry, kody odpowiedzi).
- Szerokie wsparcie narzędziowe: generatory klienta/serwera, dokumentacja interaktywna (Swagger UI/Redoc), testy.
- Ułatwia komunikację między zespołami oraz automatyzację CI/CD.

## Konsekwencje

- Specyfikacje endpointów będą zapisane w plikach OpenAPI (yaml/json) i wersjonowane w repo.
- Automatyczne generowanie kodu serwera w go.
- Dodanie kroku CI do walidacji OpenAPI i generowania dokumentacji oraz (opcjonalnie) klientów.


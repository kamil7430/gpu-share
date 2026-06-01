# Sprint 6 Planning

Sprint 6
Okres: 06-01 -- 06-07

---

## Zakres sprintu

### User story 1
Integracja frontendu z backendem.
Jako użytkownik aplikacji webowej chcę móc:

- [ ] zarejestrować swoje karty graficzne
- [ ] szukać dostępne karty
- [ ] zakładać konto i logować się


### User story 2
Jako atakujący mam dostęp do wewnętrznego REST API (api <-> coordinator).
Zatem mogę:

- wysłać zadanie do dowolnego gpu bez opłacenia zamówienia
- wykonać atak DOS
- dostać potencjalnie prywante informacje o kartach graficznych/statusach zadań

Zadania:

- [ ] dodać autektykację tokenem JWT do REST API api <-> coordinator
- [ ] zastanowić się czy udostępniać porty 2138 i 2139
- [ ] dodać testy poprawnej i błędnej autentykacji do endpointów
- [ ] dodatkowo zrefaktorować autentykację coordinatora aby nie musiał porozumiewać
      się z api (dodać klucz publiczny do weryfikacji JWT przez coordinatora)
- [ ] zaktualizować dokumentację (pliki `README` i `gpu.http`) i dodać pliki ADR.

Ryzyka:

- Zmiany dotykają dużego wycinka systemu, więc istnieje ryzyko, że trzeba będzie
  dokonać niespodziewanych zmian/refactoringu co wydłuży czas realizacji.
- Rozwiązanie z kluczem publicznym do weryfikacji JWT może nie działać, w takim
  wypadku można użyć klucza prywantego.

Story points: 5

## Kryteria realizacji

- Kod znajduje się na `main`
- Pełne testy integracyjne
- Aktualna dokumentacja

## Oczekiwany rezultat

- Atakujący próbując dostać się do REST API (api <-> coordinator) dostaje błąd
  autoryzacji
- Coordinator nie wywołuje api

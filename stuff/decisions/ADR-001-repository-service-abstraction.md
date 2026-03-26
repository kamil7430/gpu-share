## Status

Zaakceptowane

## Kontekst

Aplikacja wykorzystuje GORM jako warstwę dostępu do bazy danych.
Wprowadzono podział na:

repository – dostęp do danych (operacje CRUD)
service – logika biznesowa

Problem: operacje biznesowe często wymagają wielu wywołań repozytoriów, które powinny
być wykonane atomowo (w jednej transakcji). Jednocześnie nie chcemy, aby warstwa serwisów
była zależna od GORM (*gorm.DB).

## Decyzja

Transakcje są zarządzane w warstwie serwisów, ale realizowane przez repozytorium poprzez metodę:

```go
(r *Repository) Transaction(func(repo Repository) error) error { /* ... */ }
```

Repozytorium ukrywa szczegóły implementacyjne (GORM) i przekazuje do funkcji repozytorium powiązane z transakcją.

Serwis definiuje zakres transakcji i wykonuje w nim operacje biznesowe.

## Konsekwencje
### Zalety
- brak zależności serwisu od GORM
- jasne granice transakcji zgodne z logiką biznesową
- możliwość łatwego testowania (mockowanie repozytorium)
- enkapsulacja szczegółów bazy danych
### Wady
- dodatkowa warstwa abstrakcji
- bardziej złożony interfejs repozytorium

### Alternatywy
Przekazywanie `*gorm.DB` do serwisu

Odrzucone – narusza separację warstw i wiąże logikę biznesową z ORM.

Transakcje wewnątrz repozytorium

Odrzucone – brak możliwości łączenia wielu operacji w jedną transakcję.

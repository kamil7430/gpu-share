# 1. Product Backlog
Poniższy backlog opiera się na historyjkach użytkowników (User Stories) oraz priorytetach MoSCoW zawartych w specyfikacji.

| ID | Rola | Jako... Chcę... | Aby... | Priorytet | SP |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **PB-01** | Właściciel | Dodawać swoje karty graficzne do systemu z opisem parametrów i ceną  | Udostępniać sprzęt innym użytkownikom  | Must  | 5 |
| **PB-02** | Klient | Wyszukiwać dostępne karty graficzne według parametrów (model, VRAM, cena)  | Szybko znaleźć GPU najlepiej dopasowane do potrzeb  | Must  | 5 |
| **PB-03** | Klient | Móc wypożyczyć wybraną kartę graficzną na określony czas  | Korzystać z mocy obliczeniowej bez zakupu sprzętu  | Must  | 8 |
| **PB-04** | Właściciel | Otrzymywać automatyczne płatności po zakończeniu wypożyczenia  | Mieć gwarancję sprawiedliwego rozliczenia  | Must  | 8 |
| **PB-05** | Admin | Weryfikować konta użytkowników i właścicieli GPU  | Zapewnić bezpieczeństwo i wiarygodność platformy  | Must  | 5 |
| **PB-06** | Klient | Śledzić status aktywnego wypożyczenia (czas pozostały, wydajność)  | Mieć kontrolę nad wynajętym zasobem  | Should  | 3 |
| **PB-07** | Właściciel | Monitorować stan i obciążenie udostępnionej karty  | Upewnić się, że sprzęt jest prawidłowo używany  | Should  | 5 |
| **PB-08** | Klient | Móc zgłosić ewentualne niezgodności fizycznego GPU z ogłoszeniem  | Uniknąć nieprawidłowego obciążenia pieniężnego  | Should  | 5 |
| **PB-09** | Admin | Rozwiązywać spory między użytkownikami (nadużycia/awarie)  | Utrzymać wysoki poziom zaufania w systemie  | Should  | 8 |
| **PB-10** | Klient | Wystawić opinię po zakończeniu wypożyczenia  | Pomóc innym użytkownikom w wyborze zaufanych właścicieli  | Could  | 3 |
| **PB-11** | Admin | Monitorować ogólne statystyki i wykorzystanie zasobów  | Usprawniać działanie platformy i planować rozwój  | Could  | 5 |

---

# 2. Sprint 1 Backlog (Cel: Minimum Viable Product)
W pierwszym sprincie skupiamy się na absolutnych podstawach platformy (tzw. "Must"), aby udowodnić działanie rdzenia systemu: autoryzacji, dodawania GPU oraz prostej rezerwacji/wypożyczenia.

| ID Zadania | Związane z PB | Nazwa Zadania / Opis | Status | Szacowany czas / SP |
| :--- | :--- | :--- | :--- | :--- |
| **SP1-01** | *Architektura* | Inicjalizacja projektu, konfiguracja bazy danych i szkieletu API REST. | Do zrobienia | 3 SP |
| **SP1-02** | *Architektura* | Implementacja autoryzacji opartej o tokeny JWT (logowanie, rejestracja bazowa). | Do zrobienia | 5 SP |
| **SP1-03** | PB-01 | Stworzenie endpointu `POST /api/devices` do rejestracji karty przez Właściciela. | Do zrobienia | 3 SP |
| **SP1-04** | PB-02 | Stworzenie endpointu `GET /api/devices` oraz `GET /api/devices/{id}` do wyszukiwania GPU. | Do zrobienia | 3 SP |
| **SP1-05** | PB-03 | Stworzenie endpointu `POST /api/orders` inicjującego wynajem na określony czas (bez podpięcia płatności w tym sprincie). | Do zrobienia | 5 SP |
| **SP1-06** | PB-03 | Stworzenie endpointu `GET /api/orders/{id}` zwracającego status wypożyczenia. | Do zrobienia | 2 SP |


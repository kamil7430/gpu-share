### Status realizacji sprintu (część wykonana)

W tym sprincie skupiono się głównie na poprawie bezpieczeństwa (User Story 2) oraz na rozbudowie API zamówień (User Story 0). Z opisu wynika, że cele w tych obszarach zostały w pełni dowiezione.

---

### Szczegółowa ocena wykonanych punktów

#### User story 0: Dodanie brakującej funkcjonalności zamówień

* **Wykonane:** Dodano endpointy `GET /orders` oraz `GET /orders/{id}`.
* **Ocena:** Kluczowe punkty biznesowe z tej historyjki zostały zrealizowane. Pozwoli to na przeglądanie listy zamówień oraz szczegółów konkretnego zamówienia.

#### User story 2: Bezpieczeństwo wewnętrznego REST API (api <-> coordinator)

To była największa i najbardziej ryzykowna część sprintu (wyceniona na 5 Story Points), która została w całości zamknięta.

* **Wykonane:**
* Wprowadzono uwierzytelnianie tokenem JWT pomiędzy API a koordynatorem.
* Przeanalizowano kwestię wystawienia portów 2138 i 2139 (dobra praktyka weryfikacji architektury sieciowej).
* Dodano testy (zarówno typu *happy path*, jak i dla błędnej autentykacji), co bezpośrednio realizuje ogólne kryterium "Pełne testy integracyjne".
* **Architektura:** Z powodzeniem zrefaktorowano autentykację tak, aby koordynator weryfikował tokeny asymetrycznie za pomocą klucza publicznego, bez odpytywania API. To świetna decyzja – eliminuje to wąskie gardło i potencjalny punkt awarii (SPOF).
* Zaktualizowano dokumentację (`README`, pliki `.http`).

---

### Weryfikacja z Kryteriami Realizacji

Patrząc przez pryzmat ogólnych kryteriów dla zrobionych zadań:

1. **Kod znajduje się na `main`** – kod przeszedł przez Code Review i trafił do głównej gałęzi.
2. **Pełne testy integracyjne** – plus za jawne dodanie testów poprawnej i błędnej autentykacji w US2.
3. **Aktualna dokumentacja** – cel osiągnięty poprzez aktualizację plików `README`, `gpu.http`.

---

### Podsumowanie

Wykonana praca znacząco podnosi bezpieczeństwo aplikacji i domyka podstawowe API zamówień. Prace zostały udokumentowane zgodnie ze sztuką.

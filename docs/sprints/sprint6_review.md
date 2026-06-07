## Status realizacji sprintu (część wykonana)

W tym sprincie skupiono się głównie na poprawie bezpieczeństwa (User Story 2) oraz na rozbudowie API zamówień (User Story 1). Z opisu wynika, że cele w tych obszarach zostały w pełni dowiezione.

---

## Szczegółowa ocena wykonanych punktów

### User story 1: Dodanie brakującej funkcjonalności zamówień

* **Wykonane:** Dodano endpointy `GET /orders` oraz `GET /orders/{id}`.
* **Ocena:** Kluczowe punkty biznesowe z tej historyjki zostały zrealizowane. Pozwoli to na przeglądanie listy zamówień oraz szczegółów konkretnego zamówienia.

### User story 2: Bezpieczeństwo wewnętrznego REST API (api <-> coordinator)

* **Wykonane:**
* Wprowadzono uwierzytelnianie tokenem pomiędzy API a koordynatorem.
* Przeanalizowano kwestię wystawienia portów 2138 i 2139 (dobra praktyka weryfikacji architektury sieciowej).
* Dodano testy (zarówno typu *happy path*, jak i dla błędnej autentykacji), co bezpośrednio realizuje ogólne kryterium "Pełne testy integracyjne".
* **Architektura:** Z powodzeniem zrefaktorowano autentykację tak, aby koordynator weryfikował tokeny asymetrycznie za pomocą klucza publicznego, bez odpytywania API. To świetna decyzja – eliminuje to wąskie gardło i potencjalny punkt awarii (SPOF).
* Zaktualizowano dokumentację (`README`, pliki `.http`).

### User story 3: Wyszukiwanie GPU językiem naturalnym (Wycena: 8 SP)

To było najbardziej wymagające pod kątem punktowym (8 Story Points) i technologicznie zróżnicowane zadanie w tym sprincie. Połączenie stacku Python/NLP z resztą ekosystemu aplikacyjnego zostało zakończone sukcesem. Wydzielenie logiki NLP do osobnego kontenera w Dockerze chroni główną aplikację przed narzutem pamięciowym, który często generują modele językowe w Pythonie.

* **Wykonane:**
* **Implementacja seedowania bazy danych z pliku CSV:** Baza danych została zasilona gotowym zestawem kart graficznych. Daje to stabilny i powtarzalny zestaw danych testowych oraz produkcyjnych dla silnika wyszukiwania.
* **Setup Dockera z pythonowym serwerem REST:** Usługa NLP została poprawnie skonteneryzowana. Użycie Pythona jako dedykowanego mikroserwisu do obsługi AI/NLP to świetna decyzja architektoniczna (dostęp do bibliotek takich jak Hugging Face, spaCy czy scikit-learn).

* **Do zrobienia**:
* **Definicje OpenAPI:** Serwer posiada teraz jasny, formalny kontrakt API. Ułatwi to w przyszłości automatyczne generowanie klientów oraz ułatwiło integrację w tym sprincie.
* **Połączenie z frontendem:** Funkcjonalność została w pełni spięta ("end-to-end"). Użytkownik końcowy może już wpisać zapytanie tekstowe na UI i otrzymać przefiltrowaną listę kart.

---

## Weryfikacja z kryteriami realizacji

Patrząc przez pryzmat ogólnych kryteriów dla zrobionych zadań:

1. **Kod znajduje się na `main`** – kod przeszedł przez Code Review i trafił do głównej gałęzi.
2. **Pełne testy integracyjne** – plus za jawne dodanie testów poprawnej i błędnej autentykacji w US2.
3. **Aktualna dokumentacja** – cel osiągnięty poprzez aktualizację plików `README`, `gpu.http`.

---

## Podsumowanie

Wykonana praca znacząco podnosi bezpieczeństwo aplikacji i domyka podstawowe API zamówień. Prace zostały udokumentowane zgodnie ze sztuką.

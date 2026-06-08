## Status

Wstępnie zaakceptowane

## Kontekst

W ramach User Story 3 w Sprincie 6 aplikacja została rozbudowana o moduł wyszukiwania kart GPU za pomocą języka naturalnego (NLP). Ze względu na bogaty ekosystem bibliotek Data Science/ML (np. Hugging Face, spaCy, scikit-learn), naturalnym wyborem dla tej usługi był język Python.

Wymagało to wdrożenia pythonowego serwera REST, który musi:

* Obsługiwać asynchroniczne zapytania użytkowników.
* Efektywnie zarządzać operacjami I/O oraz potencjalnie blokującymi zadaniami CPU (ładowanie i odpytywanie modeli językowych).
* Łatwo integrować się z kontenerem Docker oraz komunikować się z frontendem za pomocą zdefiniowanego kontraktu OpenAPI.

Rozważano różne frameworki webowe w Pythonie (FastAPI, Flask, Tornado).

## Decyzja

Wybrano **Tornado** jako framework webowy i asynchroniczną bibliotekę sieciową do wdrożenia serwera REST obsługującego zapytania NLP.

Tornado charakteryzuje się nieblokującą pętlą zdarzeń (Event Loop), co pozwala na obsługę tysięcy jednoczesnych połączeń przy minimalnym narzucie zasobów. Dodatkowo, dzięki wbudowanemu wsparciu dla asynchroniczności, idealnie nadaje się do systemów, które muszą integrować się z zewnętrznymi modelami lub wykonywać asynchroniczne zapytania do bazy danych, zachowując przy tym lekkość i pełną kontrolę nad cyklem życia aplikacji.

## Konsekwencje

### Zalety

* **Wysoka wydajność I/O:** Dzięki nieblokującej architekturze opartej na epoll/kqueue, Tornado doskonale radzi sobie z obsługą wielu równoległych zapytań od użytkowników szukających GPU.
* **Lekkość i pełna kontrola:** Tornado dostarcza surowe, produkcyjne środowisko HTTP bez zbędnego narzutu (tzw. "bloatware"), co pozwala na precyzyjne dopasowanie zużycia pamięci RAM (krytyczne przy dużych modelach NLP).
* **Długowieczność i stabilność:** Framework jest sprawdzony w bojach przez gigantów technologicznych, co gwarantuje stabilność produkcyjną.
* **Wsparcie dla WebSockets:** Jeśli w przyszłości wyszukiwanie NLP będzie wymagało streamowania odpowiedzi (np. generowanie tekstu słowo po słowie, jak w ChatGPT), Tornado posiada natywne i bardzo wydajne wsparcie dla WebSockets.
* **Łatwa konteneryzacja:** Serwer Tornado nie wymaga zewnętrznego serwera WSGI (jak Gunicorn dla Flaska), ponieważ sam w sobie jest pełnoprawnym serwerem HTTP, co upraszcza konfigurację w pliku `Dockerfile`.

### Wady

* **Zarządzanie blokowaniem CPU:** Python posiada mechanizm GIL (Global Interpreter Lock). Ciężkie operacje matematyczne modeli NLP, jeśli nie są poprawnie delegowane (np. do `ThreadPoolExecutor`), mogą zablokować całą pętlę zdarzeń Tornado.
* **Mniejszy ekosystem wokół OpenAPI:** W przeciwieństwie do FastAPI, Tornado nie generuje automatycznie dokumentacji Swagger z kodu – definicje OpenAPI i walidację inputu należało zaimplementować ręcznie lub za pomocą zewnętrznych bibliotek.
* **Starszy styl asynchroniczności:** Mimo pełnego wsparcia dla nowej składni `async/await`, część dokumentacji i starszych bibliotek Tornado opiera się na dekoratorach i obiektach Future, co wymaga od deweloperów wyższej uwagi.

### Alternatywy

#### 1. FastAPI / Uvicorn

*Odrzucone* – Choć FastAPI oferuje świetną, automatyczną dokumentację OpenAPI, to narzut frameworka i jego zależności (Pydantic, Starlette) w połączeniu z ciężkimi bibliotekami NLP mógłby prowadzić do zbyt dużego zużycia pamięci w kontenerze. Tornado daje bardziej niskopoziomową kontrolę nad przepływem danych.

#### 2. Flask (z WSGI)

*Odrzucone* – Flask jest domyślnie synchroniczny. Aby obsłużyć ruch bez blokowania, wymagałby skomplikowanej konfiguracji z serwerem uWSGI/Gunicorn i workerami opartymi na gevent/eventlet. Tworzy to dodatkową warstwę technologiczną w kontenerze Docker i utrudnia debugowanie.

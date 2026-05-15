## Status
Wstępnie zaakceptowane

## Kontekst

Aplikacja wymaga nowoczesnego frontendu opartego na .NET. Dostępne opcje to:

- **Blazor Web App (.NET 10)** – zunifikowany model łączący rendering server-side i client-side
- **Blazor Server** – rendering po stronie serwera, UI przez SignalR
- **Blazor WebAssembly** – pełny klient działający w przeglądarce przez WASM

## Decyzja
Wybrano Blazor Web App z .NET 10 jako model hostingu frontendu.
Blazor Web App wprowadza ujednolicony model renderowania, w którym poszczególne komponenty mogą deklarować własny tryb renderowania (InteractiveServer, InteractiveWebAssembly, InteractiveAuto lub statyczny SSR). Eliminuje to konieczność wyboru jednego globalnego modelu dla całej aplikacji.

## Konsekwencje

### Zalety

- elastyczny rendering per komponent – strony statyczne (np. landing, dokumentacja) renderowane jako SSR bez JS, formularze i widżety interaktywne jako InteractiveAuto
- tryb Auto – przy pierwszym ładowaniu używa SignalR (szybki start), następnie przełącza się na WASM po pobraniu runtime'u w tle
- jeden projekt – brak potrzeby utrzymywania oddzielnych projektów .Client i .Server z ręczną synchronizacją współdzielonych modeli
- lepsza wydajność SEO i pierwszego ładowania – SSR dla stron publicznych bez kompromisów w zakresie interaktywności
- wsparcie .NET 10 – stabilna wersja LTS, pełne wsparcie dla nowych funkcji Blazor

### Wady

- wyższa złożoność modelu mentalnego – developer musi świadomie dobierać tryb renderowania dla każdego komponentu
- pułapki stanu – komponenty InteractiveAuto muszą być odporne na brak dostępu do HttpContext po stronie WASM
- wymagana ostrożność przy DI – serwisy zarejestrowane tylko server-side nie są dostępne w komponentach działających na WASM

### Alternatywy

1. Blazor Server
Odrzucone – stałe połączenie SignalR dla każdego użytkownika generuje wysokie zużycie zasobów serwera przy skalowaniu. Wrażliwość na opóźnienia sieci bezpośrednio degraduje UX. Brak możliwości działania offline.

2. Blazor WebAssembly (standalone)
Odrzucone – długi czas pierwszego ładowania związany z pobieraniem środowiska .NET do przeglądarki. Brak SSR utrudnia SEO. Wszystkie wywołania API muszą przechodzić przez sieć, co ujawnia więcej szczegółów implementacyjnych po stronie klienta.
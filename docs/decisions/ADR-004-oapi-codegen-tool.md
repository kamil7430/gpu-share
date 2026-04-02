## Status

Zaakceptowane

## Kontekst

Do generowania kodu serwera i/lub klienta na podstawie specyfikacji OpenAPI w Go rozważamy dostępne narzędzia. W szczególności analizowane były:

- oapi-codegen (oapi-gen)
- ogen

Podczas prób wykorzystania oapi-codegen napotkano istotne problemy:

- niepoprawna obsługa enumów,
- problemy z referencjami (`$ref`) do schematów zdefiniowanych w innych plikach,

## Decyzja

Wybieramy ogen jako narzędzie do generowania kodu Go na podstawie specyfikacji OpenAPI.

## Konsekwencje

- Generowanie kodu serwera/klienta w Go będzie realizowane przy użyciu ogen.
- Specyfikacja OpenAPI może być modularna (wiele plików), bez ograniczeń wynikających z generatora.
- Lepsze odwzorowanie typów (szczególnie enum) zmniejszy ryzyko błędów runtime.
- Mniejsza potrzeba ręcznej ingerencji w wygenerowany kod.
- Konieczność zapoznania zespołu z nowym narzędziem oraz jego konfiguracją.


## Status

Zaakceptowane

## Kontekst

REST API potrzebuje obsługiwać żądania przychodzące. 

Problem: Czy potrzeba frameworka do obsługi żądań, czy wystarczy wbudowana biblioteka net/http?

## Decyzja

Żądania obsługiwać będą handlery napisane z użyciem wbudowanej biblioteki net/http.

## Konsekwencje
### Zalety
- brak dodatkowych zależności
- pełna kontrola nad obsługą żądań
### Wady
- konieczność ręcznego parsowania adresu URL żądania

### Alternatywy
Użycie biblioteki gin (większa)

Odrzucone – duża zależność, "armata na muchę" -- wiele niepotrzebnych funkcji.

Użycie biblioteki chi (lżejsza)

Odrzucone – pomimo bycia lżejszą wersją, postanowiliśmy uniknąć dodatkowych dependencji i pisać własne handlery żądań.

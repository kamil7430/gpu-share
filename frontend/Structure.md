## Autoryzacja

### /login (modal)

- [x] EmailInput - Pole e-mail z walidacją formatu
- [x] PasswordInput - Pole hasła z przełącznikiem widoczności
- [ ] OAuthButton - Logowanie przez Google / GitHub (opcjonalne)
- [x] ErrorBanner - Komunikat błędu logowania (nieprawidłowe dane, konto zablokowane)
- [x] SubmitButton - Przycisk „Zaloguj się"
- [x] RegisterLink - Przekierowanie do rejestracji

### /register (modal)

- [x] UsernameInput - Imię i nazwisko
- [ ] EmailInput - E-mail z walidacją unikalności (do ustalenia)
- [x] PasswordInput - Hasło + potwierdzenie hasła, wskaźnik siły
- [ ] ~~RoleSelector - Wybór roli: Klient / Właściciel GPU (radio)~~
- [x] TermsCheckbox - Akceptacja regulaminu
- [ ] VerificationInfo - Informacja o wysłaniu e-maila weryfikacyjnego (do ustalenia)

### /search

- [x] SearchBar - Wolnotekstowe szukanie po modelu lub słowie kluczowym
- [x] FilterPanel - Filtry: model, min VRAM, maks. cena/h, lokalizacja, framework (pytorch/tf)
- [ ] SortDropdown - Sortowanie: cena rosnąco/malejąco, ocena, popularność
- [x] GPUCardGrid - Siatka kart GPU — model, VRAM, cena/h, ocena, status AVAILABLE/BUSY
- [x] PaginationBar - Stronicowanie lub infinite-scroll
- [x] EmptyState - Widok braku wyników z sugestią zmiany filtrów

### /gpu/:deviceId

- [x] GPUSpecCard - Model, VRAM, CUDA cores, driver, obsługiwane frameworki, lokalizacja?
- [ ] PricingBadge - Cena za godzinę + szacowany koszt dla wybranego czasu
- [ ] RatingWidget - Średnia ocena właściciela + lista ostatnich opinii
- [ ] LiveStatusIndicator - Aktualny stan GPU (dostępna/zajęta) — polling lub SSE
- [ ] DurationPicker - Wybór czasu trwania sesji (suwak + input numeryczny, min 1h)
- [ ] DockerImageInput - Pole na docker image (z przykładami: pytorch, tensorflow)
- [ ] RentButton - CTA „Wypożycz" — aktywny gdy GPU dostępna i saldo wystarczy
- [ ] OwnerMiniProfile - Awatar + nazwa właściciela, link do jego profilu

### /rentals/:rentalId (/orders/:orderId)

- [ ] CountdownTimer - Pozostały czas wypożyczenia (odliczanie do sekundy)
- [ ] CostMeter - Narastający koszt sesji w czasie rzeczywistym
- [ ] TelemetryChart - Wykresy: GPU util %, temp °C, pamięć — WebSocket/SSE stream
- [ ] ConnectionDetails - Host, port, protokół (WSS), przycisk kopiowania
- [ ] EndSessionButton - Przycisk zakończenia sesji z potwierdzeniem (modal)
- [ ] DisputeButton - Link „Zgłoś niezgodność" widoczny po 5 min od startu

### /rentals (/orders)

- [ ] RentalTable - Lista wypożyczeń: GPU, data, czas trwania, koszt, status (badge)
- [ ] StatusFilter - Filtr statusu: aktywne / zakończone / anulowane / spór
- [ ] ReceiptModal - Podgląd paragonu z podsumowaniem kosztów
- [ ] LeaveReviewButton - Przycisk „Wystaw opinię" dla zakończonych bez oceny


### /wallet (modal?)

- [ ] BalanceCard - Bieżące saldo konta (duża liczba), zablokowane środki
- [ ] TopUpForm - Formularz wpłaty: kwota + metoda płatności (karta/przelew)
- [ ] TransactionHistory - Lista transakcji: typ (wpłata/blokada/rozliczenie), kwota, data
- [ ] PendingReservations - Aktywne blokady środków z linkiem do zamówienia

### /disputes/new

- [ ] RentalReference - Automatyczne powiązanie z aktywnym lub ostatnim wypożyczeniem
- [ ] ReasonSelector - Typ niezgodności: sprzęt niezgodny z ofertą / awaria / problem z rozliczeniem
- [ ] DetailsTextarea - Opis problemu (min. 50 znaków)
- [ ] EvidenceUpload - Upload zrzutów ekranu lub logów (maks. 3 pliki, 5MB każdy)
- [ ] DisputeStatusTracker - Widok postępu sporu po zgłoszeniu (oś czasu stanów)

### /profile (own)

- [ ] EarningsSummary - Zarobki: dzisiaj / ten miesiąc / łącznie (metric cards)
- [ ] DeviceStatusList - Karty GPU: nazwa, status, aktualny najemca, uptime
- [ ] ActiveRentalFeed - Lista trwających wypożyczeń z licznikiem czasu
- [ ] NotificationFeed - Powiadomienia: nowe rezerwacje, zakończenia, zgłoszone spory

### devices modal

- [ ] AddDeviceButton - Przycisk otwierający formularz rejestracji nowej karty
- [ ] DeviceTable - Tabela: model, VRAM, cena/h, status, toggle dostępności
- [ ] AvailabilityToggle - Switch włącz/wyłącz udostępnianie — natychmiastowy PATCH
- [ ] EditDeviceDrawer - ~~Panel boczny~~ edycji parametrów i ceny bez przeładowania strony
- [ ] RemoveDeviceModal - Potwierdzenie usunięcia karty z systemu

### /owner/devices/new

- [ ] DeviceSpecForm - Pola: nazwa, model, VRAM, CUDA cores, cena/h, lokalizacja
- [ ] FrameworkCheckboxes - Checkboxy: PyTorch, TensorFlow, ONNX, JAX, inne
- [ ] AvailabilityScheduler - Siatka godzin dostępności (dni tygodnia × godziny)
- [ ] AgentInstallInstructions - Instrukcja instalacji Node Agenta z kopiowanym tokenem
- [ ] ValidationSummary - Podsumowanie błędów walidacji przed wysłaniem

### /owner/devices/:deviceId/telemetry (modal?)

- [ ] TempGauge - Wskaźnik temperatury GPU z progami ostrzeżenia
- [ ] UtilizationChart - Wykres wykorzystania GPU/VRAM — ostatnie 30 min, SSE stream
- [ ] HeartbeatStatus - Czas ostatniego heartbeatu agenta, status online/offline
- [ ] MetricHistory - Wybór zakresu czasu (1h/24h/7d) i eksport CSV

### /owner/earnings

- [ ] RevenueChart - Wykres słupkowy zarobków miesięcznie (ostatnie 12 mies.)
- [ ] PayoutHistory - Historia wypłat: data, kwota, status (zrealizowana/oczekuje)
- [ ] PayoutAccountForm - Formularz konta bankowego / PayPal do wypłat
- [ ] InvoiceExport - Eksport faktur / zestawień w PDF lub CSV

## Admin

### /admin/dashboard

- [ ] PlatformStats - KPI: aktywne sesje, zarejestrowani użytkownicy, GPU w katalogu, otwarte spory
- [ ] PendingVerifications - Kolejka kont czekających na weryfikację z przyciskami Zatwierdź/Odrzuć
- [ ] OpenDisputesFeed - Lista otwartych sporów z priorytetem i czasem oczekiwania
- [ ] FlaggedGPUsFeed - Karty GPU oczekujące na losową weryfikację

### /admin/users

- [ ] UserSearch - Szukanie po e-mailu, ID lub nazwie
- [ ] UserTable - Tabela z rolą, statusem, datą rejestracji i akcjami
- [ ] BlockUserAction - Blokada konta z polem powodu (wymagane)
- [ ] DeleteUserAction - Twarde usunięcie konta z potwierdzeniem dwuetapowym
- [ ] UserDetailDrawer - Panel boczny: historia wypożyczeń, spory, opinie

### /admin/disputes

- [ ] DisputeFilters - Filtr statusu: nowe / w trakcie / oczekujące na info / rozwiązane
- [ ] DisputeTable - Tabela: ID sporu, strony, powód, data zgłoszenia, status
- [ ] DisputeDetailPanel - Pełen wątek: wiadomości obu stron, logi telemetrii, załączniki
- [ ] ClarificationRequest - Formularz prośby o wyjaśnienia do klienta lub właściciela
- [ ] ResolutionForm - Decyzja: na korzyść klienta (zwrot) / właściciela, pole uzasadnienia

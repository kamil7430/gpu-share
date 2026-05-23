# GUI

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

## Strony

### /search

- [x] SearchBar - Wolnotekstowe szukanie po modelu lub słowie kluczowym
- [x] FilterPanel - Filtry: model, min VRAM, maks. cena/h, lokalizacja, framework (pytorch/tf)
- [ ] SortDropdown - Sortowanie: cena rosnąco/malejąco, ocena, popularność
- [x] GPUCardGrid - Siatka kart GPU — model, VRAM, cena/h, ocena, status AVAILABLE/BUSY
- [x] PaginationBar - Stronicowanie lub infinite-scroll
- [x] EmptyState - Widok braku wyników z sugestią zmiany filtrów

### /gpu/:deviceId

- [x] GPUSpecCard - Model, VRAM, CUDA cores, driver, obsługiwane frameworki, **lokalizacja?**
- [x] PricingBadge - Cena za godzinę + szacowany koszt dla wybranego czasu
- [x] RatingWidget - Średnia ocena właściciela + lista ostatnich opinii
- [x] LiveStatusIndicator - Aktualny stan GPU (dostępna/zajęta) — polling lub SSE
- [x] DurationPicker - Wybór czasu trwania sesji (suwak + input numeryczny, min 1h)
- [x] DockerImageInput - Pole na docker image (z przykładami: pytorch, tensorflow)
- [x] RentButton - CTA „Wypożycz" — aktywny gdy GPU dostępna i saldo wystarczy
- [x] OwnerMiniProfile - Awatar + nazwa właściciela, link do jego profilu

### /rentals/:rentalId (/orders/:orderId)

- [x] CountdownTimer - Pozostały czas wypożyczenia (odliczanie do sekundy)
- [x] CostMeter - Narastający koszt sesji w czasie rzeczywistym
- [x] TelemetryChart - Wykresy: GPU util %, temp °C, pamięć — WebSocket/SSE stream
- [x] ConnectionDetails - Host, port, protokół (WSS), przycisk kopiowania
- [x] EndSessionButton - Przycisk zakończenia sesji z potwierdzeniem (modal)
- [x] DisputeButton - Link „Zgłoś niezgodność" widoczny po 5 min od startu

### /rentals (/orders)

- [x] RentalTable - Lista wypożyczeń: GPU, data, czas trwania, koszt, status (badge)
- [x] StatusFilter - Filtr statusu: aktywne / zakończone / anulowane / spór
- [ ] ~~ReceiptModal - Podgląd paragonu z podsumowaniem kosztów~~ (optional)
- [x] LeaveReviewButton - Przycisk „Wystaw opinię" dla zakończonych bez oceny


### /wallet (modal?)

- [x] BalanceCard - Bieżące saldo konta (duża liczba), zablokowane środki
- [x] TopUpForm - Formularz wpłaty: kwota + metoda płatności (karta/przelew)
- [x] TransactionHistory - Lista transakcji: typ (wpłata/blokada/rozliczenie), kwota, data
- [x] PendingReservations - Aktywne blokady środków z linkiem do zamówienia

### /disputes/new

- [x] RentalReference - Automatyczne powiązanie z aktywnym lub ostatnim wypożyczeniem
- [x] ReasonSelector - Typ niezgodności: sprzęt niezgodny z ofertą / awaria / problem z rozliczeniem
- [x] DetailsTextarea - Opis problemu (min. 50 znaków)
- [x] EvidenceUpload - Upload zrzutów ekranu lub logów (maks. 3 pliki, 5MB każdy)
- [x] DisputeStatusTracker - Widok postępu sporu po zgłoszeniu (oś czasu stanów)

### /profile (own)

- [ ] ~~EarningsSummary - Zarobki: dzisiaj / ten miesiąc / łącznie (metric cards)~~ (optional)
- [x] DeviceStatusList - Karty GPU: nazwa, status, aktualny najemca, uptime
- [x] ActiveRentalFeed - Lista trwających wypożyczeń z licznikiem czasu
- [ ] ~~NotificationFeed - Powiadomienia: nowe rezerwacje, zakończenia, zgłoszone spory~~ (optional)

### devices modal

- [x] AddDeviceButton - Przycisk otwierający formularz rejestracji nowej karty
- [x] DeviceTable - Tabela: model, VRAM, cena/h, status, toggle dostępności
- [x] AvailabilityToggle - Switch włącz/wyłącz udostępnianie — natychmiastowy PATCH
- [x] EditDeviceDrawer - ~~Panel boczny~~ edycji parametrów i ceny bez przeładowania strony
- [x] RemoveDeviceModal - Potwierdzenie usunięcia karty z systemu

### /owner/devices/new

- [x] DeviceSpecForm - Pola: nazwa, model, VRAM, CUDA cores, cena/h, lokalizacja
- [x] FrameworkCheckboxes - Checkboxy: PyTorch, TensorFlow, ONNX, JAX, inne
- [ ] ~~AvailabilityScheduler - Siatka godzin dostępności (dni tygodnia × godziny)~~ (optional)
- [x] AgentInstallInstructions - Instrukcja instalacji Node Agenta z kopiowanym tokenem
- [x] ValidationSummary - Podsumowanie błędów walidacji przed wysłaniem

### /owner/devices/:deviceId/telemetry (modal?)

- [x] TempGauge - Wskaźnik temperatury GPU z progami ostrzeżenia
- [x] UtilizationChart - Wykres wykorzystania GPU/VRAM — ostatnie 30 min, SSE stream
- [x] HeartbeatStatus - Czas ostatniego heartbeatu agenta, status online/offline
- [x] MetricHistory - Wybór zakresu czasu (1h/24h/7d) i eksport CSV

### /owner/earnings

- [ ] ~~RevenueChart - Wykres słupkowy zarobków miesięcznie (ostatnie 12 mies.)~~ (optional)
- [x] PayoutHistory - Historia wypłat: data, kwota, status (zrealizowana/oczekuje)
- [x] PayoutAccountForm - Formularz konta bankowego / PayPal do wypłat
- [ ] ~~InvoiceExport - Eksport faktur / zestawień w PDF lub CSV (optional)~~

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


### Do ogarnięcia wizualnie:
- wallet
- orders table
- badge/chip


# Serwisy

### authService (API)

- [ ] login(email, password): Promise - POST /auth/login — zwraca JWT + refresh token
- [ ] register(payload): Promise - POST /auth/register — tworzy konto, wysyła e-mail weryfikacyjny
- [ ] refreshToken(): Promise - Ciche odświeżenie access tokena przed wygaśnięciem
- [ ] logout(): void - Unieważnienie tokena, czyszczenie store'u
- [ ] getMe(): Promise - GET /auth/me — profil zalogowanego użytkownika

zależy od: apiClient tokenStore

### deviceService (API)

- [ ] searchDevices(filters): Promise> - GET /api/devices z parametrami filtrów i stronicowaniem
- [ ] getDevice(deviceId): Promise - GET /api/devices/:id — szczegóły karty
- [ ] getDeviceStatus(deviceId): Promise - GET /api/devices/:id/status — polling co 30s gdy brak SSE
- [ ] registerDevice(cmd): Promise - POST /api/devices — rejestracja nowej karty przez właściciela
- [ ] updateDevice(deviceId, cmd): Promise - PATCH /api/devices/:id — edycja parametrów i ceny
- [ ] setAvailability(deviceId, available): Promise - PATCH /api/devices/:id/availability — toggle dostępności
- [ ] removeDevice(deviceId): Promise - DELETE /api/devices/:id

zależy od: apiClient

### rentalService (API)

- [ ] createRental(cmd): Promise - POST /api/orders — inicjuje wypożyczenie, zwraca connection_details
- [ ] getRental(rentalId): Promise - GET /api/orders/:id — aktualny stan wypożyczenia
- [ ] listRentals(params): Promise> - GET /api/orders — historia z filtrem statusu
- [ ] endRental(rentalId): Promise - POST /api/orders/:id/end — przedwczesne zakończenie sesji

zależy od: apiClient

### disputeService (API)

- [ ] openDispute(cmd): Promise - POST /api/disputes — zgłoszenie niezgodności
- [ ] getDispute(disputeId): Promise - GET /api/disputes/:id — stan sporu
- [ ] listDisputes(params): Promise> - GET /api/disputes — lista dla admina z filtrami
- [ ] submitClarification(disputeId, payload): Promise - POST /api/disputes/:id/clarification — odpowiedź strony sporu
- [ ] resolveDispute(disputeId, decision): Promise - POST /api/disputes/:id/resolve — tylko admin

zależy od: apiClient

### paymentService (API)

- [ ] getBalance(): Promise - GET /api/wallet — saldo i zablokowane środki
- [ ] topUp(amount, method): Promise - POST /api/wallet/topup — inicjuje płatność przez PSP
- [ ] getTransactions(params): Promise> - GET /api/wallet/transactions — historia transakcji
- [ ] getPayouts(ownerId): Promise> - GET /api/owner/payouts — historia wypłat właściciela

zależy od: apiClient

### reviewService (API)

- [ ] createReview(rentalId, cmd): Promise - POST /api/orders/:id/review — jednorazowa ocena po sesji
- [ ] getDeviceReviews(deviceId): Promise - GET /api/devices/:id/reviews — opinie dla strony szczegółów GPU

zależy od: apiClient

### adminService (API)

- [ ] listUsers(params): Promise> - GET /api/admin/users z filtrem statusu i roli
- [ ] verifyUser(userId, verdict): Promise - POST /api/admin/users/:id/verify — zatwierdź / odrzuć konto
- [ ] blockUser(userId, reason): Promise - POST /api/admin/users/:id/block
- [ ] getPlatformStats(): Promise - GET /api/admin/stats — KPI dla dashboardu admina
- [ ] verifyDevice(deviceId, verdict): Promise - POST /api/admin/devices/:id/verify — losowa weryfikacja GPU

zależy od: apiClient

### authStore (Stan)

- [ ] user: User | null - Zalogowany użytkownik (null gdy niezalogowany)
- [ ] accessToken: string | null - JWT access token trzymany w pamięci (nie localStorage)
- [ ] role: "client"|"owner"|"admin"|null - Aktywna rola — steruje routingiem i widocznością elementów
- [ ] login(email, password): Promise - Wywołuje authService.login i zapisuje token w store
- [ ] logout(): void - Czyści store i przekierowuje na /login

zależy od: authService

### rentalStore (Stan)

- [ ] activeRental: Rental | null - Aktualnie trwające wypożyczenie bieżącego użytkownika
- [ ] telemetry: TelemetrySnapshot[] - Bufor ostatnich N próbek dla wykresów w aktywnej sesji
- [ ] startRental(cmd): Promise - Tworzy rental, podłącza SSE, aktualizuje activeRental
- [ ] endRental(): Promise - Zamyka SSE, wysyła END, czyści activeRental

zależy od: rentalService telemetryService

### deviceStore (Stan)

- [ ] myDevices: GPUDevice[] - Karty GPU zalogowanego właściciela
- [ ] fetchMyDevices(): Promise - Ładuje listę i zapisuje w store
- [ ] toggleAvailability(deviceId): Promise - Optimistic update toggle + PATCH do API

ależy od: deviceService

### telemetryService (Real-time)

- [ ] subscribe(deviceId, onSnapshot): Unsubscribe - SSE /api/devices/:id/telemetry — stream metryk GPU
- [ ] unsubscribe(deviceId): void - Zamknięcie EventSource po odmontowaniu komponentu
- [ ] fallbackPoll(deviceId, interval): Unsubscribe - Polling GET /status co N ms gdy SSE niedostępne
- [ ] onError(handler): void - Callback na utratę połączenia — wyświetla baner w UI

zależy od: authStore

### notificationService (Real-time)

- [ ] connect(): void - Otwiera SSE /api/notifications po zalogowaniu
- [ ] onNotification(handler): Unsubscribe - Subskrypcja powiadomień — nowe rezerwacje, spory, wypłaty
- [ ] disconnect(): void - Zamknięcie połączenia przy wylogowaniu

zależy od: authStore

### apiClient (Narzędziowy)

request(config): Promise - Axios/fetch wrapper — dołącza Bearer token, obsługuje 401 → refresh
onError interceptor - Mapuje kody HTTP na czytelne błędy: 409 Conflict, 4xx, 5xx
retry(n): void - Automatyczny retry dla 5xx z exponential backoff
zależy od: authStore

### routerGuard (Narzędziowy)

requireAuth(to, next) - Redirect na /login gdy brak tokena
requireRole(roles[])(to, next) - Redirect na /403 gdy rola użytkownika nie jest w liście
redirectIfLoggedIn(to, next) - Redirect z /login na stronę główną gdy już zalogowany
zależy od: authStore

### formatters (Narzędziowy)

formatUSD(amount): string - Intl.NumberFormat — "$0.45" bez rounding artefaktów
formatDuration(seconds): string - "2h 34m 12s" — dla CountdownTimer i historii
formatDateTime(iso): string - Locale-aware data i godzina ("6 sty 2026, 12:34")
formatVRAM(mb): string - "24 GB" / "512 MB" — dla kart GPU w katalogu

### validationSchemas (Narzędziowy)

registerSchema - e-mail, hasło ≥8 znaków, wybór roli
deviceSchema - model wymagany, vram_mb > 0, price_per_hour_usd > 0
rentalSchema - device_id uuid, docker_image niepusty, duration_hours 1–720
disputeSchema - reason enum, details min 50 znaków

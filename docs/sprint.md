## Sprint Goal

Replace frontend mock data with real backend API communication and deliver an end-to-end functional MVP.

## Epic 1: Authentication Integration

### User Stories
- [x] Integrate login endpoint
- [ ] Integrate logout endpoint
- [x] Persist JWT/token
- [ ] Restore session after page refresh
- [x] Protect authorized pages

### Tasks
- [x] Implement login request
- [ ] Implement logout request
- [x] Store token
- [x] Configure HttpClient authorization handler
- [x] Connect AuthState
- [x] Remove authentication mocks
- [ ] Test authorized/unauthorized flows

### Definition of Done
- [ ] User can log in
- [ ] User can log out
- [ ] Session survives refresh
- [ ] Protected pages work correctly

## Epic 2: Device Management Integration

### User Stories
- [ ] User can view devices
- [ ] User can add device
- [ ] User can edit device
- [ ] User can remove device

### Tasks
- [x] Create DeviceApiClient
- [x] Integrate GpuList
- [x] Integrate DeviceCard
- [x] Integrate DevicePage
- [x] Integrate EditDeviceForm
- [ ] Integrate remove modal
- [ ] Replace mock telemetry retrieval

### Definition of Done
- [ ] Device CRUD works against backend
- [ ] No device mocks remain

## Epic 3: Search Integration

### User Stories
- [ ] User can search GPUs
- [ ] User can filter GPUs

### Tasks
- [ ] Connect SearchBar
- [ ] Convert filters into API query parameters
- [ ] Integrate pagination
- [ ] Integrate sorting

### Definition of Done
- [ ] Search results come from backend
- [ ] Filters affect API results

## Epic 4: Order Integration

### User Stories
- [ ] User can create order
- [ ] User can view order
- [ ] User can finish order

### Tasks
- [ ] Connect DeviceOrderForm
- [ ] Connect reservation validation
- [ ] Connect OrderPage
- [ ] Connect DeviceStatsCard
- [ ] Connect ConnectionCard
- [ ] Connect session ending flow

### Definition of Done
- [ ] Complete ordering flow works
- [ ] User can start and finish session

## Epic 5: Reservation Calendar Integration

### User Stories
- [ ] User can see real reservations

### Tasks
- [ ] Connect ReservationCalendar
- [ ] Implement week navigation API calls
- [ ] Handle empty weeks
- [ ] Handle overlapping reservations

### Definition of Done
- [ ] Calendar displays real order data

## Epic 6: Reviews Integration

### User Stories
- [ ] User can view reviews
- [ ] User can create reviews

### Tasks
- [ ] Connect ReviewsList
- [ ] Connect review modal
- [ ] Implement paging
- [ ] Implement load more

### Definition of Done
- [ ] Reviews are fully backend-driven

## Epic 7: Dispute System Integration

- [ ] User Stories
- [ ] User can submit dispute
- [ ] User can upload evidence

### Tasks
- [ ] Connect DisputeForm
- [ ] Implement file upload API
- [ ] Connect dispute submission
- [ ] Connect dispute history

### Definition of Done
- [ ] Disputes are persisted in backend

## Epic 8: Telemetry Integration

### User Stories
- [ ] User can view real telemetry

### Tasks
- [ ] Connect TelemetryCard
- [ ] Connect OrderTelemetryCard
- [ ] Implement polling/SSE/WebSocket
- [ ] Implement CSV export

### Definition of Done
- [ ] Telemetry displays live backend data

## Technical Tasks

### API Infrastructure
- [ ] Create typed API clients
- [ ] Add global exception handling
- [ ] Add loading states
- [ ] Add retry policies
- [ ] Add notification system (success/error toasts)

### Logging
- [ ] Add service-level logging
- [ ] Add API failure logging
- [ ] Add telemetry error logging

### Testing
- [ ] Replace mocks with API mocks
- [ ] Add integration tests
- [ ] Verify end-to-end flows

## Sprint Deliverable

By the end of the sprint, a user should be able to:

1. Register/Login
2. Add a GPU
3. Search available GPUs
4. Reserve a GPU
5. View live session information
6. Leave a review
7. Open a dispute
8. View telemetry

with all data coming from the backend and no frontend mock data remaining.

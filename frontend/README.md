# GpuShare Frontend

A modern, interactive web frontend for GPU resource sharing and management built with Blazor and .NET.

## Tech Stack

### Framework & Runtime
- **ASP.NET Core** (.NET 10.0)
- **Blazor Interactive Server** - Interactive web UI with C# and server-side rendering
- **Razor Components** - Component-based UI architecture

### UI & Styling
- **Blazorise** (v2.1.0) - UI component library for Blazor
- **MudBlazor** (v9.4.0) - Material-inspired UI components and services
- **Bootstrap 5** - Responsive layout and utilities
- **FontAwesome Icons** - Icon support for UI elements
- **CSS** - Custom app styling

### Development
- **C#** - Primary application language
- **Nullable Reference Types** enabled
- **Implicit Using Statements** enabled

## Project Structure

```
frontend/
├── Components/
│   ├── App.razor
│   ├── Routes.razor
│   ├── _Imports.razor
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── MainLayout.razor.css
│   │   ├── ReconnectModal.razor
│   │   └── ReconnectModal.razor.css
│   ├── Modals/
│   │   ├── BaseModal.razor
│   │   ├── BaseModal.razor.css
│   │   ├── ChangePasswordModal.razor
│   │   ├── ChangePasswordModal.razor.css
│   │   ├── EndSessionModal.razor
│   │   ├── EndSessionModal.razor.css
│   │   ├── FilterModal.razor
│   │   ├── FilterModal.razor.css
│   │   ├── LoginModal.razor
│   │   ├── LoginModal.razor.css
│   │   ├── RemoveDeviceModal.razor
│   │   └── RemoveDeviceModal.css
│   ├── Pages/
│   │   ├── Device/
│   │   │   ├── Device.razor
│   │   │   ├── Device.razor.css
│   │   │   ├── Calendar.razor
│   │   │   ├── Calendar.razor.css
│   │   │   ├── DeviceInfo.razor
│   │   │   ├── DeviceInfo.razor.css
│   │   │   ├── EditDeviceForm.razor
│   │   │   ├── EditDeviceForm.razor.css
│   │   │   ├── TelemetryCard.razor
│   │   │   └── TelemetryCard.razor.css
│   │   ├── Devices/
│   │   │   ├── Devices.razor
│   │   │   ├── Devices.razor.css
│   │   │   ├── SearchBar.razor
│   │   │   └── SearchBar.razor.css
│   │   ├── Dispute/
│   │   │   ├── Dispute.razor
│   │   │   ├── Dispute.razor.css
│   │   │   ├── DisputeForm.razor
│   │   │   ├── DisputeForm.razor.css
│   │   │   ├── Timeline.razor
│   │   │   └── Timeline.razor.css
│   │   ├── Order/
│   │   │   ├── Order.razor
│   │   │   ├── Order.razor.css
│   │   │   ├── OrderConnection.razor
│   │   │   ├── OrderConnection.razor.css
│   │   │   ├── OrderDeviceStats.razor
│   │   │   ├── OrderDeviceStats.razor.css
│   │   │   ├── OrderTelemetry.razor
│   │   │   └── OrderTelemetry.razor.css
│   │   └── Profile/
│   │       ├── Profile.razor
│   │       ├── Profile.razor.css
│   │       ├── ProfileCard.razor
│   │       ├── ProfileCard.razor.css
│   │       ├── OrderTable.razor
│   │       ├── OrderTable.razor.css
│   │       ├── WalletCard.razor
│   │       └── WalletCard.razor.css
│   └── Shared/
│       ├── CheckBoxList.razor
│       ├── GpuCard.razor
│       ├── GpuCard.razor.css
│       ├── GpuList.razor
│       ├── GpuList.razor.css
│       ├── GpuOrderForm.razor
│       ├── GpuOrderForm.razor.css
│       ├── OpinionsList.razor
│       └── OpinionsList.razor.css
├── Models/
│   ├── Gpu.cs
│   ├── Opinion.cs
│   ├── Order.cs
│   ├── SearchFilter.cs
│   └── User.cs
├── Services/
├── Properties/
│   └── launchSettings.json
├── wwwroot/
│   ├── app.css
│   └── lib/bootstrap/
├── Program.cs
├── GpuShare.Frontend.csproj
├── appsettings.json
└── appsettings.Development.json
```

## Key Features

### Pages
- **Devices** - Browse and filter available GPU resources
- **Device** - View device details, telemetry, and calendar availability
- **Order** - Manage orders with connection info, device statistics, and telemetry
- **Profile** - Manage user account details and order history
- **Dispute** - Track dispute workflow with timeline and form support
- **Error / NotFound** - Friendly error handling pages for invalid routes

### Shared Components
- **GpuCard** - GPU specification and availability card
- **GpuList** - GPU catalog listing
- **GpuOrderForm** - Order creation and checkout UI
- **OrderTable** - Order history and transaction display
- **OpinionsList** - Reviews and ratings presentation
- **CheckBoxList** - Multi-select filter interface
- **WalletCard** / **TelemetryCard** - Account and telemetry summaries

### Page-specific UI
- **Device page** includes calendar, telemetry, and editable device details
- **Order page** includes connection and device stats panels
- **Profile page** includes profile summary, orders, and wallet view
- **Dispute page** includes dispute form and timeline tracking

### Architecture
- **Component-based UI** for reusable, maintainable views
- **Responsive design** for desktop and mobile experiences
- **Interactive Server Rendering** with Blazor interactive server components
- **Static assets** served from `wwwroot` and component libraries

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later
- Visual Studio or VS Code with C# tooling

### Run locally

1. Open a terminal in the project root
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Start the app:
   ```bash
   dotnet run
   ```
4. Open the browser at the URL shown in the terminal

### Build for production

```bash
dotnet build -c Release
```

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Blazorise | 2.1.0 | Core Blazor UI components |
| Blazorise.Bootstrap | 2.1.0 | Bootstrap provider for Blazorise |
| Blazorise.Bootstrap5 | 2.1.0 | Bootstrap 5 styling integration |
| Blazorise.Components | 2.1.0 | Additional Blazorise UI components |
| Blazorise.Icons.FontAwesome | 2.1.0 | FontAwesome icon support |
| MudBlazor | 9.4.0 | Material-style components and services |

## Configuration

### appsettings.json
Production configuration values.

### appsettings.Development.json
Development-specific configuration values.

### launchSettings.json
Launch profiles and environment settings.

## Notes

- HTTPS redirect is disabled for Docker scenarios in `Program.cs`
- `BlazorDisableThrowNavigationException` is enabled in project settings
- The app uses `MapRazorComponents` / `AddInteractiveServerRenderMode` in `Program.cs`

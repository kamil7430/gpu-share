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
│   ├── App.razor                # Root HTML shell and component mount
│   ├── Routes.razor             # Router configuration
│   ├── _Imports.razor           # Shared imports for components
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── MainLayout.razor.css
│   │   ├── ReconnectModal.razor
│   │   └── ReconnectModal.razor.css
│   ├── Pages/
│   │   ├── Device.razor
│   │   ├── Device.razor.css
│   │   ├── Devices.razor
│   │   ├── Devices.razor.css
│   │   ├── Dispute.razor
│   │   ├── Dispute.razor.css
│   │   ├── Error.razor
│   │   ├── NotFound.razor
│   │   ├── Order.razor
│   │   ├── Order.razor.css
│   │   ├── Profile.razor
│   │   └── Profile.razor.css
│   └── Shared/
│       ├── ChangePasswordModal.razor
│       ├── CheckBoxList.razor
│       ├── EditDeviceForm.razor
│       ├── EditDeviceForm.razor.css
│       ├── FilterModal.razor
│       ├── FilterModal.razor.css
│       ├── GpuCard.razor
│       ├── GpuCard.razor.css
│       ├── GpuList.razor
│       ├── GpuList.razor.css
│       ├── GpuOrderForm.razor
│       ├── GpuOrderForm.razor.css
│       ├── LoginModal.razor
│       ├── LoginModal.razor.css
│       ├── OpinionsCard.razor
│       ├── OpinionsCard.razor.css
│       ├── OrderTable.razor
│       ├── OrderTable.razor.css
│       ├── TelemetryCard.razor
│       ├── TelemetryCard.razor.css
│       ├── WalletCard.razor
│       └── WalletCard.razor.css
├── Models/
│   ├── Gpu.cs
│   ├── Opinion.cs
│   ├── Order.cs
│   ├── SearchFilter.cs
│   └── User.cs
├── Services/                    # Business logic and API service implementations
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
- **Devices** - Browse available GPU resources
- **Device** - Detailed GPU device and availability view
- **Order** - Place and manage GPU rental orders
- **Profile** - User profile and account details
- **Dispute** - Dispute management workflow
- **Error / NotFound** - Friendly error handling pages

### Shared Components
- **GpuCard** - GPU specification and availability card
- **GpuList** - GPU catalog listing
- **GpuOrderForm** - Order checkout form
- **OrderTable** - Order history and transaction display
- **OpinionsCard** - Review and rating component
- **WalletCard** / **TelemetryCard** - Wallet and telemetry summaries
- **EditDeviceForm** - Device editing UI
- **Modal dialogs** - Login, filtering, password changes, reconnect handling
- **CheckBoxList** - Multi-select filtering UI

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

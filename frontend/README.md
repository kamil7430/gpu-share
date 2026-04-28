# GpuShare Frontend

A modern, interactive web application for GPU resource sharing and management built with Blazor and .NET.

## Tech Stack

### Framework & Runtime
- **ASP.NET Core** (.NET 10.0) - High-performance web framework
- **Blazor Interactive Server** - Interactive web UI with C# instead of JavaScript
- **Razor Components** - Component-based architecture

### UI & Styling
- **Blazorise** (v2.1.0) - Rich component library for Blazor
- **Bootstrap 5** - Responsive CSS framework
- **FontAwesome Icons** - Icon library for UI elements
- **CSS** - Custom styling for components

### Development
- **C#** - Primary programming language
- **Nullable Reference Types** - Type safety
- **Implicit Using Statements** - Cleaner code

## Project Structure

```
frontend/
├── Components/                 # Blazor components and pages
│   ├── App.razor              # Root component
│   ├── Routes.razor           # Route definitions
│   ├── _Imports.razor         # Global imports
│   │
│   ├── Layout/                # Application layouts
│   │   ├── MainLayout.razor   # Main layout component
│   │   ├── MainLayout.razor.css
│   │   ├── ReconnectModal.razor
│   │   └── ReconnectModal.razor.css
│   │
│   ├── Pages/                 # Page components
│   │   ├── Device.razor       # GPU device details page
│   │   ├── Device.razor.css
│   │   ├── Profile.razor      # User profile page
│   │   ├── Profile.razor.css
│   │   ├── Search.razor       # GPU search page
│   │   ├── Search.razor.css
│   │   ├── Error.razor        # Error page
│   │   └── NotFound.razor     # 404 page
│   │
│   └── Shared/                # Reusable UI components
│       ├── ChangePasswordModal.razor
│       ├── CheckBoxList.razor
│       ├── FilterModal.razor
│       ├── FilterModal.razor.css
│       ├── GpuCard.razor      # GPU information card
│       ├── GpuCard.razor.css
│       ├── LoginModal.razor
│       ├── LoginModal.razor.css
│       ├── OpinionsCard.razor # Opinion/review component
│       ├── OpinionsCard.razor.css
│       ├── OrderTable.razor   # Order list display
│       └── OrderTable.razor.css
│
├── Models/                    # Data models (C#)
│   ├── Gpu.cs
│   ├── Opinion.cs
│   ├── Order.cs
│   ├── SearchFilter.cs
│   └── User.cs
│
├── Services/                  # Business logic and API services
│   └── [Service implementations]
│
├── Properties/               # Application properties
│   └── launchSettings.json   # Run configurations
│
├── wwwroot/                  # Static files (CSS, images, scripts)
│   ├── app.css              # Global styles
│   └── lib/                 # Third-party libraries
│       └── bootstrap/       # Bootstrap CSS framework
│
├── bin/                      # Build output
├── obj/                      # Compiler output
│
├── Program.cs               # Application entry point & configuration
├── GpuShare.Frontend.csproj  # Project configuration
├── appsettings.json         # Application settings
└── appsettings.Development.json # Development-specific settings
```

## Key Features

### Pages
- **Device Page** - View detailed GPU device information
- **Profile Page** - User profile management
- **Search Page** - Discover and filter available GPUs

### Shared Components
- **GpuCard** - Displays GPU specifications and availability
- **OrderTable** - Shows user orders and transactions
- **OpinionsCard** - User reviews and ratings
- **Modals** - Login, password change, and filtering dialogs
- **CheckBoxList** - Multi-select filter interface

### Architecture
- **Component-Based UI** - Reusable, maintainable components
- **Responsive Design** - Works on desktop and mobile
- **Interactive Server Rendering** - Real-time updates without page refresh

## Getting Started

### Prerequisites
- .NET 10.0 or later
- Visual Studio or VS Code with C# extension

### Development

1. **Navigate to the frontend directory:**
   ```bash
   cd frontend
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Build for production:**
   ```bash
   dotnet build -c Release
   ```

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Blazorise | 2.1.0 | UI Component Library |
| Blazorise.Bootstrap | 2.1.0 | Bootstrap Integration |
| Blazorise.Bootstrap5 | 2.1.0 | Bootstrap 5 Components |
| Blazorise.Components | 2.1.0 | Additional Components |
| Blazorise.Icons.FontAwesome | 2.1.0 | Icon Support |

## Configuration

### appsettings.json
Contains production configuration settings.

### appsettings.Development.json
Contains development-specific configuration (debugging, logging, etc.).

### launchSettings.json
Defines project launch profiles and environment variables.

## Development Notes

- **Nullable Reference Types** are enabled for better null safety
- **Implicit Using Statements** are enabled to reduce boilerplate
- The application disables HTTPS redirection for Docker containers
- Custom navigation exceptions in Blazor are disabled

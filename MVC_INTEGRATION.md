# MVC Integration with Blazor 9

This application now supports both ASP.NET Core MVC and Blazor components running side-by-side.

## Architecture

The application follows the integration pattern described in [Microsoft's Blazor Component Integration documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/integration?view=aspnetcore-9.0).

### Key Components

1. **Program.cs Configuration**
   - `AddControllersWithViews()` - Adds MVC services
   - `AddRazorPages()` - Adds Razor Pages support
   - `MapControllerRoute()` - Maps traditional MVC routes
   - `MapRazorPages()` - Maps Razor Pages routes
   - `MapRazorComponents<App>()` - Maps Blazor components

2. **MVC Structure**
   - `Controllers/` - Contains MVC controllers (e.g., HomeController)
   - `Views/` - Contains Razor views (.cshtml files)
   - `Views/Shared/` - Shared layouts and partial views
   - `Views/_ViewImports.cshtml` - Global imports for views
   - `Views/_ViewStart.cshtml` - Default layout configuration

## Routing

- **MVC Routes**: `/Home/Index`, `/Home/About` (follows pattern: `/{controller}/{action}`)
- **Blazor Routes**: `/` (root) and all other routes defined in Blazor components

## Usage

### Access MVC Views
Navigate to:
- http://localhost:5168/Home/Index - MVC home page
- http://localhost:5168/Home/About - MVC about page

### Access Blazor App
Navigate to:
- http://localhost:5168/ - Blazor application root

## Benefits of This Integration

1. **Gradual Migration**: Existing MVC applications can gradually adopt Blazor
2. **Best of Both Worlds**: Use MVC for traditional request/response scenarios, Blazor for rich interactive experiences
3. **Shared Infrastructure**: Both MVC and Blazor share the same middleware pipeline and services
4. **SEO Friendly**: MVC views can provide server-rendered content for better SEO

## Development

The application uses .NET 9 and follows standard ASP.NET Core conventions:
- Controllers inherit from `Controller`
- Views use Razor syntax (.cshtml)
- Blazor components use Razor component syntax (.razor)

## Building and Running

```bash
# Build the solution
dotnet build RFPAPP.sln

# Run the application
cd RFPResponsePOC/RFPResponsePOC
dotnet run
```

The application will start on http://localhost:5168 (or https://localhost:7150 for HTTPS).

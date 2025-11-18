# ClientHub FYI - Proof of Concept

A .NET 10 Blazor Web App that integrates with Microsoft Entra ID, FYI Docs External API, Microsoft Graph, and provides an MCP server for LLM integration.

## Features

- **Microsoft Entra ID Authentication**: Single-tenant authentication with Microsoft Entra ID
- **Fluent UI Blazor**: Modern, responsive UI using Microsoft's Fluent UI components
- **FYI Docs Integration**: Search clients, view jobs, documents, and tasks from FYI Docs
- **Microsoft Graph Integration**: View calendar events and tasks from Microsoft 365
- **MCP Server**: Model Context Protocol server exposing FYI and calendar data to LLMs

## Prerequisites

Before you begin, ensure you have the following:

- **.NET 10 SDK** installed ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
  - .NET 10 is the latest Long Term Support (LTS) release, released November 12, 2025
  - Supported until November 10, 2028
  - Includes C# 14 and major performance improvements
- **Microsoft Entra ID** tenant with admin access to create app registrations
- **FYI Docs** account with access to the External API
- **(Optional)** MCP-compatible client like Claude Desktop for testing the MCP server

## Project Structure

```
ClientHubFyiPoc/
├── ClientHubFyi.Web/           # Main Blazor Web App project
│   ├── Components/
│   │   ├── Layout/             # Layout components (MainLayout, NavMenu)
│   │   └── Pages/              # Razor pages (Home, Calendar, Tasks, FYI, etc.)
│   ├── Models/                 # Data models
│   ├── Services/               # Service implementations
│   │   ├── FyiApiClient.cs     # FYI Docs API client
│   │   ├── GraphService.cs     # Microsoft Graph service
│   │   ├── McpServerService.cs # MCP server service
│   │   └── McpServerHostedService.cs  # MCP background service
│   ├── wwwroot/                # Static files
│   ├── Program.cs              # Application entry point
│   └── appsettings.json        # Configuration (template)
├── mcp-config-example.json     # Example MCP client configuration
└── README.md                   # This file
```

## Setup Instructions

### 1. Microsoft Entra ID Application Registration

#### Step 1: Register the Application

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com/)
2. Navigate to **Identity** > **Applications** > **App registrations**
3. Click **New registration**
4. Configure the registration:
   - **Name**: `ClientHubFyiPoc`
   - **Supported account types**: `Accounts in this organizational directory only (Single tenant)`
   - **Redirect URI**:
     - Platform: `Web`
     - URI: `https://localhost:5001/signin-oidc`
5. Click **Register**

#### Step 2: Configure Authentication

1. In your app registration, go to **Authentication**
2. Under **Implicit grant and hybrid flows**, check:
   - ✅ **ID tokens** (used for implicit and hybrid flows)
3. Set **Front-channel logout URL**: `https://localhost:5001/signout-oidc`
4. Click **Save**

#### Step 3: Add API Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph** > **Delegated permissions**
4. Add the following permissions:
   - `User.Read` (usually added by default)
   - `Calendars.Read`
   - `Tasks.Read`
5. Click **Add permissions**
6. Click **Grant admin consent for [Your Tenant]** and confirm

#### Step 4: Create a Client Secret (Optional)

If you need app-only access (for background services), create a client secret:

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Add a description and select an expiration period
4. Click **Add**
5. **Copy the secret value immediately** (you won't be able to see it again)

#### Step 5: Note Your Configuration Values

From the **Overview** page, note down:
- **Application (client) ID**: `[Your Client ID]`
- **Directory (tenant) ID**: `[Your Tenant ID]`
- **Tenant domain**: `[Your Tenant].onmicrosoft.com`

### 2. FYI Docs API Configuration

#### Step 1: Register Your Application in FYI Docs

1. Sign in to your FYI Docs account
2. Navigate to the External API settings
3. Follow the documentation at [https://developers.fyi.app/](https://developers.fyi.app/) to:
   - Create an API application
   - Generate API credentials (API Key and API Secret)

#### Step 2: Note Your FYI API Configuration

- **Base URL**: `https://api.fyi.app` (or your FYI instance URL)
- **API Key**: `[Your API Key]`
- **API Secret**: `[Your API Secret]`

### 3. Application Configuration

#### Option A: Using User Secrets (Recommended for Development)

1. Navigate to the project directory:
   ```bash
   cd ClientHubFyi.Web
   ```

2. Initialize user secrets:
   ```bash
   dotnet user-secrets init
   ```

3. Set the configuration values:
   ```bash
   # Azure AD / Entra ID
   dotnet user-secrets set "AzureAd:Domain" "[YourTenant].onmicrosoft.com"
   dotnet user-secrets set "AzureAd:TenantId" "[Your Tenant ID]"
   dotnet user-secrets set "AzureAd:ClientId" "[Your Client ID]"
   dotnet user-secrets set "AzureAd:ClientSecret" "[Your Client Secret]"

   # FYI API
   dotnet user-secrets set "FyiApi:BaseUrl" "https://api.fyi.app"
   dotnet user-secrets set "FyiApi:ApiKey" "[Your FYI API Key]"
   dotnet user-secrets set "FyiApi:ApiSecret" "[Your FYI API Secret]"
   ```

#### Option B: Using appsettings.Development.json

Create a file `ClientHubFyi.Web/appsettings.Development.json` (this file is gitignored):

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "[YourTenant].onmicrosoft.com",
    "TenantId": "[Your Tenant ID]",
    "ClientId": "[Your Client ID]",
    "ClientSecret": "[Your Client Secret]",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  },
  "MicrosoftGraph": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": "user.read calendars.read tasks.read"
  },
  "FyiApi": {
    "BaseUrl": "https://api.fyi.app",
    "ApiKey": "[Your FYI API Key]",
    "ApiSecret": "[Your FYI API Secret]"
  },
  "McpServer": {
    "Enabled": true,
    "Port": 3000,
    "Name": "ClientHub FYI MCP Server"
  }
}
```

### 4. Build and Run

#### Restore Dependencies

```bash
dotnet restore
```

#### Build the Project

```bash
dotnet build
```

#### Run the Application

```bash
cd ClientHubFyi.Web
dotnet run
```

Or use the HTTPS profile:

```bash
dotnet run --launch-profile https
```

The application will start and be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

Open your browser and navigate to `https://localhost:5001`

### 5. Verify Authentication

1. Click **Sign in** in the top right corner
2. You'll be redirected to Microsoft's login page
3. Sign in with your Microsoft 365 account
4. Grant consent for the requested permissions
5. You'll be redirected back to the application

### 6. Explore the Features

#### Home Page
- Overview of available features
- Quick links to Calendar, Tasks, and FYI Clients

#### Calendar
- View your Microsoft 365 calendar events for the next 7 days
- Filter events by subject
- Click links to join online meetings

#### Tasks
- View your Microsoft To Do tasks
- See task status, priority, and due dates
- View all your task lists

#### FYI Clients
- Search for clients by name or code
- View client details including contacts
- Explore client jobs with documents and tasks in a master-detail view

#### Profile
- View your authentication claims
- Verify your identity information

### 7. Using the MCP Server

The MCP server runs automatically when the application starts (if enabled in configuration).

#### Available MCP Tools

1. **search_fyi_clients**
   - Description: Search for FYI clients by name or code
   - Input: `{ "query": "string" }`
   - Output: JSON array of matching clients

2. **get_fyi_client_overview**
   - Description: Get comprehensive client overview with jobs and documents
   - Input: `{ "clientId": "string" }`
   - Output: JSON object with client details, jobs, documents, and tasks

3. **get_calendar_events**
   - Description: Get calendar events for the next N days
   - Input: `{ "daysAhead": 7 }` (optional)
   - Output: JSON array of calendar events

#### Connecting an MCP Client

##### For Claude Desktop:

1. Locate your Claude Desktop configuration file:
   - **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
   - **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
   - **Linux**: `~/.config/Claude/claude_desktop_config.json`

2. Add the server configuration (see `mcp-config-example.json`):

```json
{
  "mcpServers": {
    "clienthub-fyi": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/ClientHubFyi.Web/ClientHubFyi.Web.csproj"
      ],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

3. Restart Claude Desktop

4. Test the connection by asking Claude to:
   - "Search for FYI clients matching 'Acme'"
   - "Get the overview for client ID '1'"
   - "Show my calendar events for the next 7 days"

## Troubleshooting

### Authentication Issues

**Problem**: "AADSTS50011: The redirect URI specified in the request does not match..."

**Solution**: Ensure the redirect URI in your Entra app registration matches exactly:
- `https://localhost:5001/signin-oidc` (note HTTPS and port)

**Problem**: "Insufficient privileges to complete the operation"

**Solution**: Grant admin consent for the Microsoft Graph permissions in the Entra admin center.

### FYI API Issues

**Problem**: Getting mock data instead of real FYI data

**Solution**: This is expected behavior when the FYI API is not configured or unreachable. The app falls back to mock data for demonstration purposes. To connect to the real API:
1. Verify your FYI API credentials are correct
2. Check the FYI API base URL
3. Ensure your FYI account has API access enabled

### MCP Server Issues

**Problem**: MCP server not starting

**Solution**:
1. Verify `"McpServer:Enabled"` is set to `true` in your configuration
2. Check the application logs for any startup errors
3. Ensure port 3000 (or your configured port) is not in use

**Note**: The MCP server implementation in this POC is a placeholder. For a full implementation, you'll need to integrate the official MCP C# SDK.

## Architecture Notes

### Authentication Flow

1. User clicks "Sign in"
2. Application redirects to Microsoft Entra ID
3. User authenticates and grants consent
4. Entra ID redirects back with authorization code
5. Application exchanges code for access token
6. Token is cached and used for Microsoft Graph API calls

### FYI API Integration

- The `FyiApiClient` handles authentication and API calls
- Implements retry logic with Polly for resilience
- Falls back to mock data if API is unavailable (for demo purposes)
- Uses strongly-typed models for all FYI entities

### Microsoft Graph Integration

- Uses `Microsoft.Identity.Web` for seamless token acquisition
- Delegates permissions ensure API calls are made on behalf of the signed-in user
- `GraphService` provides a clean abstraction over the Graph SDK

### MCP Server

- Runs as a `BackgroundService` alongside the Blazor app
- Exposes three tools for LLM integration
- `McpServerService` serializes responses to JSON for LLM consumption
- **Note**: This is a conceptual implementation; full MCP SDK integration is needed for production use

## Development Notes

### Adding New Pages

1. Create a new Razor component in `Components/Pages/`
2. Add the `@page` directive with your route
3. Add `@attribute [Authorize]` if authentication is required
4. Add a navigation link in `Components/Layout/NavMenu.razor`

### Adding New Services

1. Create an interface in `Services/` (e.g., `IMyService.cs`)
2. Implement the service (e.g., `MyService.cs`)
3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IMyService, MyService>();
   ```

### Extending the MCP Server

To add new MCP tools:
1. Add method to `IMcpServerService` interface
2. Implement the method in `McpServerService`
3. Register the tool in `McpServerHostedService` (when using full MCP SDK)

## Security Considerations

- **Never commit secrets**: Use user secrets or environment variables
- **Client secrets**: Rotate regularly and store securely
- **API keys**: Treat FYI API keys as sensitive credentials
- **Token caching**: In-memory cache is used; consider distributed cache for production
- **HTTPS**: Always use HTTPS in production
- **CORS**: Configure appropriately for your deployment scenario

## References

- [ASP.NET Core Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Blazor Web App Security with Entra ID](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/blazor-web-app-with-entra?view=aspnetcore-10.0)
- [Microsoft Graph Documentation](https://learn.microsoft.com/en-us/graph/)
- [Fluent UI Blazor](https://www.fluentui-blazor.net/)
- [FYI Docs External API](https://developers.fyi.app/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)

## License

This is a proof of concept project. Check with your organization regarding licensing and usage policies.

## Support

For issues or questions:
- Review the troubleshooting section above
- Check the application logs for detailed error messages
- Consult the official documentation for each integrated service

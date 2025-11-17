using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Graph;
using Microsoft.FluentUI.AspNetCore.Components;
using ClientHubFyi.Web.Components;
using ClientHubFyi.Web.Services;
using ClientHubFyi.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Azure AD authentication
var initialScopes = builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ') ?? new[] { "user.read" };

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add Fluent UI Blazor components
builder.Services.AddFluentUIComponents();

// Configure strongly typed settings
builder.Services.Configure<FyiApiSettings>(
    builder.Configuration.GetSection("FyiApi"));
builder.Services.Configure<McpServerSettings>(
    builder.Configuration.GetSection("McpServer"));

// Add HTTP clients with Polly for resilience
builder.Services.AddHttpClient<IFyiApiClient, FyiApiClient>()
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// Add application services
builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddScoped<IMcpServerService, McpServerService>();

// Add MCP Server as a hosted service if enabled
if (builder.Configuration.GetValue<bool>("McpServer:Enabled"))
{
    builder.Services.AddHostedService<McpServerHostedService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();

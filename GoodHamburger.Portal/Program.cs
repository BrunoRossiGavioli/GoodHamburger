using GoodHamburger.Portal.Components;
using GoodHamburger.Portal.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Add authentication and authorization
// ProtectedLocalStorage: compartilhado entre abas do mesmo domínio (origin),
// criptografado pela Data Protection API. Substitui o ProtectedSessionStorage
// que era isolado por aba/circuito.
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<ILocalStorageService, ProtectedSessionStorageService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// Configure HttpClient with TokenRefreshHandler
builder.Services.AddScoped<TokenRefreshHandler>();
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl is not configured."));
}).AddHttpMessageHandler<TokenRefreshHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Davivienda.FrontEnd;
using Davivienda.GraphQL.SDK;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
// Usamos SessionStorage para cumplir el requisito de cerrar sesión al cerrar el navegador
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Davivienda.FrontEnd.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient base
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// --- REGISTRO DE ALMACENAMIENTO ---
// Importante para que el token pida login en otra PC o al cerrar navegador
builder.Services.AddBlazoredSessionStorage();

// --- CONFIGURACIÓN DE SEGURIDAD .NET 8 ---
builder.Services.AddAuthorizationCore();

// Vinculamos el CustomAuthStateProvider como el proveedor de estado oficial
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Permite usar NotifyLogin/NotifyLogout directamente desde el código del Login
builder.Services.AddScoped<CustomAuthStateProvider>(sp =>
    (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
// -------------------------------------------

// REGISTRO DEL CLIENTE GRAPHQL
builder.Services.AddDaviviendaGraphQLClient()
    .ConfigureHttpClient(client =>
    {
        // Asegúrate de que este puerto sea el de tu Backend
        client.BaseAddress = new Uri("http://localhost:5098/graphql");
    });

await builder.Build().RunAsync();
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Davivienda.FrontEnd;
using Davivienda.GraphQL.SDK;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Blazored.LocalStorage;
// NUEVOS USINGS PARA SEGURIDAD
using Microsoft.AspNetCore.Components.Authorization;
using Davivienda.FrontEnd.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient para llamadas convencionales
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Registrar el servicio de Local Storage
builder.Services.AddBlazoredLocalStorage();

// --- CONFIGURACIÓN DE SEGURIDAD (PASO 2) ---
// 1. Habilita el sistema de autorización principal [cite: 2026-02-10]
builder.Services.AddAuthorizationCore();

// 2. Vincula tu clase CustomAuthStateProvider como el proveedor oficial [cite: 2026-02-10]
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
// -------------------------------------------

// REGISTRO DEL CLIENTE GRAPHQL
builder.Services.AddDaviviendaGraphQLClient()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5098/graphql");
    });

await builder.Build().RunAsync();
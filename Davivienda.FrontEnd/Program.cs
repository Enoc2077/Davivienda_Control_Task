using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Davivienda.FrontEnd;
using Davivienda.GraphQL.SDK;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
// Paso 1: Importar el namespace del paquete instalado
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient para llamadas convencionales
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Paso 2: Registrar el servicio de Local Storage en el contenedor de dependencias
builder.Services.AddBlazoredLocalStorage();

// REGISTRO DEL CLIENTE GRAPHQL
builder.Services.AddDaviviendaGraphQLClient()
    .ConfigureHttpClient(client =>
    {
        // Asegúrate de que esta URL sea la correcta para tu servidor backend
        client.BaseAddress = new Uri("http://localhost:5098/graphql");
    });

await builder.Build().RunAsync();
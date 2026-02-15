using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Davivienda.FrontEnd;
using Davivienda.GraphQL.SDK; // Namespace definido en tu .graphqlrc.json
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient para llamadas convencionales
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// REGISTRO DEL CLIENTE GRAPHQL
// Strawberry Shake genera este método basado en el "name" de tu archivo JSON
builder.Services.AddDaviviendaGraphQLClient()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5098/graphql");
    });

await builder.Build().RunAsync();
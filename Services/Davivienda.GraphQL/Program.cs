using Davivienda.GraphQL.DataBases;
using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.GraphQL.ServicesQuery.Type;
using Davivienda.GraphQL.ServicesQuery.Type.Mutation;
using Davivienda.GraphQL.ServicesQuery.Type.Mutations;
using Davivienda.GraphQL.ServicesQuery.Type.Query;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Serialization;
// NUEVOS USINGS PARA SEGURIDAD
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Davivienda.GraphQL.Security;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. REGISTRO DE SERVICIOS
builder.Services.AddSingleton<DataBase>();
builder.Services.AddSingleton<JwtProvider>(); // REGISTRO DEL PROVEEDOR DE TOKENS

// Registro de Builders y Services (Tus servicios existentes)
builder.Services.AddTransient<AreaQueryBuilder>();
builder.Services.AddTransient<AreaServices>();
builder.Services.AddScoped<BitacoraFriccionQueryBuilder>();
builder.Services.AddScoped<BitacoraFriccionServices>();
builder.Services.AddScoped<BitacoraSolucionesQueryBuilder>();
builder.Services.AddScoped<BitacoraSolucionesServices>();
builder.Services.AddScoped<ComentariosQueryBuilder>();
builder.Services.AddScoped<ComentariosServices>();
builder.Services.AddScoped<DetalleProyectoQueryBuilder>();
builder.Services.AddScoped<DetalleProyectoServices>();
builder.Services.AddScoped<DocumentacionQueryBuilder>();
builder.Services.AddScoped<DocumentacionServices>();
builder.Services.AddScoped<FriccionQueryBuilder>();
builder.Services.AddScoped<FriccionServices>();
builder.Services.AddScoped<NotificacionesQueryBuilder>();
builder.Services.AddScoped<NotificacionesServices>();
builder.Services.AddScoped<PrioridadQueryBuilder>();
builder.Services.AddScoped<PrioridadServices>();
builder.Services.AddScoped<ProcesoQueryBuilder>();
builder.Services.AddScoped<ProcesoServices>();
builder.Services.AddScoped<ProyectosQueryBuilder>();
builder.Services.AddScoped<ProyectosServices>();
builder.Services.AddScoped<RolesQueryBuilder>();
builder.Services.AddScoped<RolesServices>();
builder.Services.AddScoped<SolucionesQueryBuilder>();
builder.Services.AddScoped<SolucionesServices>();
builder.Services.AddScoped<TareaQueryBuilder>();
builder.Services.AddScoped<TareaServices>();
builder.Services.AddScoped<UsuarioQueryBuilder>();
builder.Services.AddScoped<UsuarioServices>();

// 2.1 CONFIGURACIÓN DE AUTENTICACIÓN JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// 3. CONFIGURACIÓN DE HOT CHOCOLATE
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
        .AddTypeExtension<AreaQuery>()
        .AddTypeExtension<BitacoraFriccionQuery>()
        .AddTypeExtension<BitacoraSolucionesQuery>()
        .AddTypeExtension<ComentariosQuery>()
        .AddTypeExtension<DetalleProyectoQuery>()
        .AddTypeExtension<DocumentacionQuery>()
        .AddTypeExtension<FriccionQuery>()
        .AddTypeExtension<NotificacionesQuery>()
        .AddTypeExtension<PrioridadQuery>()
        .AddTypeExtension<ProcesoQuery>()
        .AddTypeExtension<ProyectosQuery>()
        .AddTypeExtension<RolesQuery>()
        .AddTypeExtension<SolucionesQuery>()
        .AddTypeExtension<TareaQuery>()
        .AddTypeExtension<UsuarioQuery>()
    .AddMutationType(d => d.Name("Mutation"))
        .AddTypeExtension<AreaMutation>()
        .AddTypeExtension<BitacoraFriccionMutation>()
        .AddTypeExtension<BitacoraSolucionesMutation>()
        .AddTypeExtension<ComentariosMutation>()
        .AddTypeExtension<DetalleProyectoMutation>()
        .AddTypeExtension<DocumentacionMutation>()
        .AddTypeExtension<FriccionMutation>()
        .AddTypeExtension<NotificacionesMutation>()
        .AddTypeExtension<PrioridadMutation>()
        .AddTypeExtension<ProcesoMutation>()
        .AddTypeExtension<ProyectosMutation>()
        .AddTypeExtension<RolesMutation>()
        .AddTypeExtension<SolucionesMutation>()
        .AddTypeExtension<TareaMutation>()
        .AddTypeExtension<UsuarioMutation>()
   .AddSubscriptionType<Suscripciones>()
   .AddFiltering()
   .AddSorting()
   .AddInMemorySubscriptions()
   .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

// 4. PIPELINE DE MIDDLEWARE
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

// El orden aquí es crítico: CORS -> Authentication -> Authorization
app.UseCors("Dev");

app.UseAuthentication(); // NUEVO
app.UseAuthorization();  // NUEVO

app.UseWebSockets();
app.MapGraphQL();

app.Run();
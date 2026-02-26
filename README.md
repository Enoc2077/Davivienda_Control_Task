# Davivienda Control Task

Este proyecto utiliza Entity Framework Core. Para configurar el entorno local después de clonar, sigue estas instrucciones:

## Configuración de Base de Datos

1. **Cadena de Conexión:**
   Asegúrate de configurar tu servidor local en el archivo `appsettings.json` dentro del proyecto Davivienda/GraphQL.

2. **Aplicar Migraciones (Importante):**
   Para crear las tablas y cargar los datos iniciales (Seed Data), se necesita ejecutar el siguiente comando en la terminal desde la raíz del proyecto de Davivienda/Migrations:

   ```powershell
   dotnet ef database update

3. Credenciales de Acceso
Usuario: admin
Numero de usuario: 00001
Contraseña:  Admin123

4. Requisitos Previos
* .NET 8 SDK
* SQL Server (Express lo tengo yo)
* EF Core Tools instalado: `dotnet tool install --global dotnet-ef`

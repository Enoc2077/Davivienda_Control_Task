using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace Davivienda.GraphQL.DataBases
{
    public class DataBase : IDisposable
    {
        private SqlConnection? connection;

        // Propiedad para obtener la conexión activa
        public SqlConnection? Connection { get => this.connection; }

        // Constructor que carga la configuración (Simulado según tu oficina)
        public DataBase()
        {
            // Nota: En tu oficina usan un 'LoadConfig'. Aquí lo adaptamos al estándar de .NET
            // para que use el ConnectionString que pusimos en el appsettings.json
            string stringConexion = "Server=DESKTOP-IJ2LO3K\\SQLEXPRESS;Database=Davivienda_Asignaciones;Trusted_Connection=True;TrustServerCertificate=True;";
            connection = new SqlConnection(stringConexion);
        }

        // Métodos de gestión de estado
        public void Connect() => this.connection?.Open();

        public async Task ConnectAsync()
        {
            // Solo intentamos abrir si la conexión NO está ya abierta
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                await Connection.OpenAsync();
            }
        }

        public void Disconnect() => this.connection?.Close();

        public async Task DisconnectAsync() => await this.connection?.CloseAsync();

        // Implementación de IDisposable para liberar recursos
        public void Dispose()
        {
            this.connection?.Dispose();
        }
    }

    // Enum para manejar distintos tipos de bases de datos si fuera necesario
    public enum DataBaseType
    {
        AS400,
        SQL_Server
    }
} 
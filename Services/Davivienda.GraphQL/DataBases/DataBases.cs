using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace Davivienda.GraphQL.DataBases
{
    // IAsyncDisposable para que el contenedor de servicios
    public class DataBase : IDisposable, /*/ /*/ IAsyncDisposable
    {
        private SqlConnection? connection;

        // Propiedad para obtener la conexión activa lo encapsulamos
        public SqlConnection? Connection { get => this.connection; }

        public DataBase()
        {
            // Mantenemos tu cadena de conexión 
            string stringConexion = "Server=DESKTOP-IJ2LO3K\\SQLEXPRESS;Database=Davivienda_Asignaciones;Trusted_Connection=True;TrustServerCertificate=True;";
            connection = new SqlConnection(stringConexion);
        }

        // Métodos de gestión de estado
        public void Connect()
        {
            if (connection != null && connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        public async Task ConnectAsync()
        {
            // Solo intentamos abrir si la conexión NO está ya abierta
            if (connection != null && connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
        }

        public void Disconnect() => this.connection?.Close();

        public async Task DisconnectAsync()
        {
            if (this.connection != null)
            {
                await this.connection.CloseAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
                connection = null;
            }
        }

        // Limpieza Síncrona
        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }
    }

    public enum DataBaseType
    {
        AS400,
        SQL_Server
    }
}
using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Asignaciones
    {
        // 1. INJECT
        [Inject] private DaviviendaGraphQLClient Client { get; set; }

        // 2. PARAMETERS
        [Parameter] public List<TareaModel>? Tasks { get; set; }
        [Parameter] public List<PrioridadModel>? Prioridades { get; set; }

        // 3. PROPERTIES (Lógica de estado)
        public UsuarioModel? UsuarioActual { get; set; }
        public List<TareaModel> TareasFiltradas { get; set; } = new();
        public string PrioridadSeleccionada { get; set; } = "Todas";

        // Propiedad calculada para las iniciales del usuario de la base de datos
        public string Initials => GetInitials(UsuarioActual?.USU_NOM);

        // 4. FUNCTIONS
        protected override async Task OnInitializedAsync()
        {
            await CargarDatosDesdeBase();
        }

        private async Task CargarDatosDesdeBase()
        {
            try
            {
                // Llamada a la Query 1: GetTareas
                var response = await Client.GetTareas.ExecuteAsync();

                if (response.Data != null && response.Data.Tareas != null)
                {
                    // Asignamos los registros reales de la base de datos
                    Tasks = response.Data.Tareas.Select(t => new TareaModel
                    {
                        TAR_ID = t.Tar_ID,
                        TAR_NOM = t.Tar_NOM,
                        TAR_DES = t.Tar_DES,
                        TAR_EST = t.Tar_EST,
                        PRI_ID = t.Pri_ID,
                        USU_ID = t.Usu_ID,
                        TAR_FEC_INI = t.Tar_FEC_INI
                    }).ToList();

                    // Aquí deberías cargar también tu lista de Prioridades desde la base
                    // para que el filtro funcione con lo que tengas guardado.
                    // Si solo tienes "Alta", solo eso se mostrará.

                    TareasFiltradas = Tasks;
                }

                // Simulación de usuario (aquí deberías usar una query de Usuario si la tienes)
                // Por ahora, para que no salga vacío:
                UsuarioActual = new UsuarioModel { USU_NOM = "Usuario DB" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
            }
        }

        private void FiltrarPorPrioridad(string nombrePrioridad)
        {
            PrioridadSeleccionada = nombrePrioridad;

            if (nombrePrioridad == "Todas")
            {
                TareasFiltradas = Tasks ?? new();
            }
            else
            {
                // Filtramos comparando contra el nombre de la prioridad
                // Nota: Requiere que tengas cargada la lista 'Prioridades' de la DB
                TareasFiltradas = Tasks?.Where(t => GetPrioridadNombre(t.PRI_ID) == nombrePrioridad).ToList() ?? new();
            }
        }

        public string GetPrioridadNombre(Guid? priId)
        {
            if (priId == null || Prioridades == null) return "Baja";
            return Prioridades.FirstOrDefault(p => p.PRI_ID == priId)?.PRI_NOM ?? "Baja";
        }

        private string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "DB";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1
                ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                : $"{parts[0][0]}".ToUpper();
        }
    }
}
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Davivienda.GraphQL.SDK;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Tarea
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; }
        [Inject] private IJSRuntime JS { get; set; }

        // --- Listas de Datos ---
        private List<TareaModel> TareasLista { get; set; } = new();
        private List<ProyectosModel> ProyectosLista { get; set; } = new();
        private List<ProcesoModel> TodosLosProcesos { get; set; } = new();
        private List<ProcesoModel> ProcesosFiltrados { get; set; } = new();

        // --- Variables de Estado y Formulario ---
        private string NuevaTareaNombre { get; set; } = "";
        private string NuevaTareaDesc { get; set; } = "";
        private string SelectedProyectoId { get; set; } = "";
        private string SelectedProcesoId { get; set; } = "";

        private bool Cargando { get; set; } = true;
        private bool IsProcessing { get; set; } = false; // Control del botón guardar

        // --- Propiedades para el Modal de Edición ---
        private bool MostrarModal { get; set; } = false;
        private TareaModel TareaEdicion { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await CargarTareas();
            await CargarDatosFiltros();
        }

        private async Task CargarTareas()
        {
            try
            {
                Cargando = true;
                var res = await Client.GetTareas.ExecuteAsync();
                if (res.Data?.Tareas != null)
                {
                    TareasLista = res.Data.Tareas.Select(t => new TareaModel
                    {
                        TAR_ID = t.Tar_ID,
                        TAR_NOM = t.Tar_NOM,
                        TAR_DES = t.Tar_DES,
                        PROC_ID = t.Proc_ID,
                        TAR_FEC_FIN = t.Tar_FEC_FIN
                    }).ToList();
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error al cargar tareas: {ex.Message}"); }
            finally { Cargando = false; }
        }

        private async Task CargarDatosFiltros()
        {
            try
            {
                // Cargar Proyectos para el primer Select
                var resProy = await Client.GetProyectos.ExecuteAsync();
                if (resProy.Data?.Proyectos != null)
                {
                    ProyectosLista = resProy.Data.Proyectos.Select(p => new ProyectosModel
                    {
                        PRO_ID = p.Pro_ID,
                        PRO_NOM = p.Pro_NOM
                    }).ToList();
                }

                // Cargar Procesos para el filtrado en cascada
                var resProc = await Client.GetProcesos.ExecuteAsync();
                if (resProc.Data?.Procesos != null)
                {
                    TodosLosProcesos = resProc.Data.Procesos.Select(p => new ProcesoModel
                    {
                        PROC_ID = p.Proc_ID,
                        PROC_NOM = p.Proc_NOM,
                        PRO_ID = p.Pro_ID
                    }).ToList();
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error en filtros: {ex.Message}"); }
        }

        private void OnProyectoChanged(ChangeEventArgs e)
        {
            SelectedProyectoId = e.Value?.ToString() ?? "";
            SelectedProcesoId = ""; // Resetear selección de proceso

            if (!string.IsNullOrEmpty(SelectedProyectoId))
            {
                // Filtrar procesos que pertenecen al proyecto seleccionado
                ProcesosFiltrados = TodosLosProcesos
                    .Where(p => p.PRO_ID.ToString().ToUpper() == SelectedProyectoId.ToUpper())
                    .ToList();
            }
            else
            {
                ProcesosFiltrados.Clear();
            }
        }

        private async Task GuardarNuevaTarea()
        {
            if (string.IsNullOrEmpty(SelectedProcesoId) || string.IsNullOrWhiteSpace(NuevaTareaNombre)) return;

            try
            {
                IsProcessing = true; // Bloquea el botón visualmente

                var input = new TareaModelInput
                {
                    Tar_ID = Guid.NewGuid(),
                    Tar_NOM = NuevaTareaNombre,
                    Tar_DES = NuevaTareaDesc,
                    Tar_EST = "Pendiente",
                    Proc_ID = Guid.Parse(SelectedProcesoId),
                    Tar_FEC_CRE = DateTimeOffset.Now,
                    Tar_FEC_INI = DateTimeOffset.Now // Campo requerido corregido
                };

                var res = await Client.InsertTarea.ExecuteAsync(input);
                if (res.Errors.Count == 0)
                {
                    await CargarTareas();
                    // Limpiamos campos de texto pero mantenemos el proceso para seguir guardando
                    NuevaTareaNombre = "";
                    NuevaTareaDesc = "";
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error al insertar: {ex.Message}"); }
            finally
            {
                IsProcessing = false; // Desbloquea el botón para permitir más registros
            }
        }

        // --- Lógica de Edición (Modal) ---
        private void AbrirModalEdicion(TareaModel tarea)
        {
            // Creamos una copia para no editar la lista principal antes de guardar
            TareaEdicion = new TareaModel
            {
                TAR_ID = tarea.TAR_ID,
                TAR_NOM = tarea.TAR_NOM,
                TAR_DES = tarea.TAR_DES,
                PROC_ID = tarea.PROC_ID
            };
            MostrarModal = true;
        }

        private async Task GuardarEdicion()
        {
            try
            {
                // El error "required field missing" ocurre porque el input de edición 
                // también exige los campos obligatorios del esquema.
                var input = new TareaModelInput
                {
                    Tar_ID = TareaEdicion.TAR_ID,
                    Tar_NOM = TareaEdicion.TAR_NOM,
                    Tar_DES = TareaEdicion.TAR_DES,
                    // Agregamos los campos obligatorios para evitar el error de GraphQL
                    Tar_FEC_INI = DateTimeOffset.Now,
                    Tar_FEC_CRE = DateTimeOffset.Now,
                    Tar_EST = "Pendiente"
                };

                var res = await Client.UpdateTarea.ExecuteAsync(input);

                if (res.Errors.Count == 0)
                {
                    await CargarTareas();
                    MostrarModal = false;
                }
                else
                {
                    foreach (var error in res.Errors)
                    {
                        Console.WriteLine($"Error de GraphQL al editar: {error.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar: {ex.Message}");
            }
        }

        // --- Lógica de Eliminación ---
        private async Task EliminarTarea(Guid id)
        {
            bool confirm = await JS.InvokeAsync<bool>("confirm", "¿Está seguro de que desea eliminar esta tarea?");
            if (confirm)
            {
                try
                {
                    var res = await Client.DeleteTarea.ExecuteAsync(id);
                    if (res.Errors.Count == 0)
                    {
                        await CargarTareas();
                    }
                }
                catch (Exception ex) { Console.WriteLine($"Error al eliminar: {ex.Message}"); }
            }
        }
    }
}
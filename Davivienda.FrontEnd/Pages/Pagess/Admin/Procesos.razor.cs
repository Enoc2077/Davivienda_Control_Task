using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public class ProcesoBase : ComponentBase
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;

        [Parameter] public Guid ProyectoId { get; set; }
        [Parameter] public string ProyectoNombre { get; set; } = "";
        [Parameter] public EventCallback OnClose { get; set; }

        public List<ProcesoModel> ProcesosDelProyecto { get; set; } = new();
        public List<TareaModel> TareasDelProceso { get; set; } = new();
        public ProcesoModel? ProcesoSeleccionado { get; set; }
        public string TextoBusquedaProceso { get; set; } = "";

        // --- ESTADOS PARA MODALES DE TAREAS ---
        public bool MostrarModalTarea { get; set; } = false;
        public bool MostrarModalCrearTarea { get; set; } = false;
        public TareaModel TareaSeleccionada { get; set; } = new();

        // Métodos para Editar
        public void AbrirEditarTarea(TareaModel tarea)
        {
            TareaSeleccionada = tarea;
            MostrarModalTarea = true;
            StateHasChanged();
        }

        public void CerrarModalTarea()
        {
            MostrarModalTarea = false;
            StateHasChanged();
        }

        // Métodos para Crear
        public void AbrirCrearTarea()
        {
            MostrarModalCrearTarea = true;
            StateHasChanged();
        }

        public async Task CerrarModalCrearTarea()
        {
            MostrarModalCrearTarea = false;
            if (ProcesoSeleccionado != null) await CargarTareasDelProceso(ProcesoSeleccionado);
            StateHasChanged();
        }
        // ---------------------------------------

        public IEnumerable<ProcesoModel> ProcesosFiltradosEnModal =>
            string.IsNullOrWhiteSpace(TextoBusquedaProceso)
                ? ProcesosDelProyecto
                : ProcesosDelProyecto.Where(p => p.PROC_NOM.Contains(TextoBusquedaProceso, StringComparison.OrdinalIgnoreCase));

        protected override async Task OnParametersSetAsync()
        {
            if (ProyectoId != Guid.Empty)
            {
                ProcesoSeleccionado = new ProcesoModel { PROC_FRE = "Diario", PROC_EST = true };
                await CargarProcesos();
            }
        }

        public async Task CargarProcesos()
        {
            try
            {
                var res = await Client.GetProcesos.ExecuteAsync();
                ProcesosDelProyecto = res.Data?.Procesos
                    .Where(p => p.Pro_ID == ProyectoId)
                    .Select(p => new ProcesoModel
                    {
                        PROC_ID = p.Proc_ID,
                        PROC_NOM = p.Proc_NOM,
                        PROC_DES = p.Proc_DES,
                        PROC_FRE = p.Proc_FRE ?? "Frecuencia no definida",
                        PROC_EST = p.Proc_EST,
                        PRO_ID = p.Pro_ID,
                        PROC_FEC_CRE = p.Proc_FEC_CRE,
                        PROC_FEC_MOD = p.Proc_FEC_MOD
                    }).ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar procesos: {ex.Message}");
            }
        }

        public async Task CargarTareasDelProceso(ProcesoModel proc)
        {
            ProcesoSeleccionado = proc; // Guardamos referencia directa del proceso seleccionado
            try
            {
                var res = await Client.GetTareas.ExecuteAsync();
                TareasDelProceso = res.Data?.Tareas
                    .Where(t => t.Proc_ID == proc.PROC_ID)
                    .Select(t => new TareaModel
                    {
                        TAR_ID = t.Tar_ID,
                        TAR_NOM = t.Tar_NOM,
                        TAR_DES = t.Tar_DES,
                        TAR_EST = t.Tar_EST,
                        PROC_ID = t.Proc_ID
                    })
                    .ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar tareas: {ex.Message}");
            }
        }

        public async Task GuardarCambiosProceso()
        {
            if (ProcesoSeleccionado == null) return;
            try
            {
                DateTimeOffset fechaCreacion = ProcesoSeleccionado.PROC_FEC_CRE == default
                    ? DateTimeOffset.Now
                    : ProcesoSeleccionado.PROC_FEC_CRE;

                var input = new ProcesoModelInput
                {
                    Proc_ID = ProcesoSeleccionado.PROC_ID,
                    Proc_NOM = ProcesoSeleccionado.PROC_NOM,
                    Proc_DES = ProcesoSeleccionado.PROC_DES,
                    Proc_FRE = ProcesoSeleccionado.PROC_FRE,
                    Proc_EST = ProcesoSeleccionado.PROC_EST ?? true,
                    Pro_ID = ProyectoId,
                    Proc_FEC_CRE = fechaCreacion,
                    Proc_FEC_MOD = DateTimeOffset.Now
                };

                if (ProcesoSeleccionado.PROC_ID == Guid.Empty) await Client.InsertProceso.ExecuteAsync(input);
                else await Client.UpdateProceso.ExecuteAsync(input);

                await CargarProcesos();
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        public void PrepararNuevoProceso()
        {
            ProcesoSeleccionado = new ProcesoModel { PROC_ID = Guid.Empty, PROC_NOM = "", PROC_FRE = "Diario", PROC_EST = true, PRO_ID = ProyectoId };
            TareasDelProceso = new();
            StateHasChanged();
        }

        public void OnSearchProcesoChanged(ChangeEventArgs e) => TextoBusquedaProceso = e.Value?.ToString() ?? "";
        public Task CerrarModal() => OnClose.InvokeAsync();
    }
}
using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class DetalleTarea : ComponentBase, IAsyncDisposable
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public TareaModel? Tarea { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private List<FriccionModel> FriccionesList = new();
        private List<SolucionesModel> SolucionesList = new();
        private string NombreProceso = "Cargando...";
        private string NombreProyecto = "Cargando...";
        private bool MostrarModalCrear = false;
        private string TipoModal = "";

        private SolucionesModel? SolucionSeleccionada;
        private FriccionModel? FriccionSeleccionada;

        private string TituloModal => TipoModal switch
        {
            "FRICCION_NUEVA" => "Nueva Fricción",
            "SOLUCION_NUEVA" => "Nueva Solución",
            "SOLUCION_EDITAR" => "Editar Solución",
            "FRICCION_EDITAR" => "Editar Fricción",
            "HISTORIAL_SOLUCIONES" => "Bitácora de Soluciones",
            "HISTORIAL_FRICCIONES" => "Bitácora de Fricciones",
            _ => "Detalle"
        };

        protected override async Task OnInitializedAsync()
        {
            if (Tarea != null)
            {
                await CargarDatosTarea();
                await CargarJerarquia();
            }
        }

        private async Task CargarJerarquia()
        {
            try
            {
                if (Tarea?.PROC_ID != null)
                {
                    var resProc = await Client.GetProcesoById.ExecuteAsync(Tarea.PROC_ID.Value);
                    if (resProc.Data?.ProcesoById != null)
                    {
                        NombreProceso = resProc.Data.ProcesoById.Proc_NOM.ToUpper();
                        var proId = resProc.Data.ProcesoById.Pro_ID;
                        if (proId.HasValue)
                        {
                            var resProy = await Client.GetProyectoById.ExecuteAsync(proId.Value);
                            NombreProyecto = resProy.Data?.ProyectoById?.Pro_NOM.ToUpper() ?? "Proyecto no encontrado";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error jerarquía: {ex.Message}");
                NombreProceso = "Error";
                NombreProyecto = "Error";
            }
            StateHasChanged();
        }

        private async Task CargarDatosTarea()
        {
            try
            {
                // 1. Cargar fricciones de esta tarea
                var resFri = await Client.GetFricciones.ExecuteAsync();
                FriccionesList = resFri.Data?.Fricciones?
                    .Where(f => f.Tar_ID == Tarea?.TAR_ID)
                    .Select(f => new FriccionModel
                    {
                        FRI_ID = f.Fri_ID,
                        FRI_TIP = f.Fri_TIP,
                        FRI_DES = f.Fri_DES,
                        FRI_EST = f.Fri_EST,
                        FRI_IMP = f.Fri_IMP,
                        TAR_ID = f.Tar_ID,
                        FRI_FEC_CRE = f.Fri_FEC_CRE.DateTime
                    }).ToList() ?? new();

                // 2. Cargar soluciones
                var resSol = await Client.GetSoluciones.ExecuteAsync();

                // IDs de las fricciones de esta tarea
                var idsFricciones = FriccionesList.Select(f => f.FRI_ID).ToHashSet();

                SolucionesList = resSol.Data?.Soluciones?
                    .Where(s =>
                        // Opcion A: la solucion tiene una friccion que pertenece a esta tarea
                        (s.Fri_ID.HasValue && idsFricciones.Contains(s.Fri_ID.Value))
                        ||
                        // Opcion B: la solucion NO tiene friccion y fue creada por el mismo usuario
                        // (filtro temporal hasta que se agregue TAR_ID a la tabla SOLUCIONES)
                        (!s.Fri_ID.HasValue && s.Usu_ID == Guid.Parse("0BC4DB21-1FFB-46BB-B120-48AE7B0909CD"))
                    )
                    .Select(s => new SolucionesModel
                    {
                        SOL_ID = s.Sol_ID,
                        SOL_NOM = s.Sol_NOM,
                        SOL_DES = s.Sol_DES,
                        SOL_EST = s.Sol_EST,
                        SOL_NIV_EFE = s.Sol_NIV_EFE,
                        FRI_ID = s.Fri_ID,
                        USU_ID = s.Usu_ID,
                        SOL_FEC_CRE = s.Sol_FEC_CRE.DateTime
                    }).ToList() ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error carga: {ex.Message}");
            }
            StateHasChanged();
        }

        // Autoguardado al cerrar
        private async Task GuardarTarea()
        {
            if (Tarea == null) return;
            try
            {
                var input = new TareaModelInput
                {
                    Tar_ID = Tarea.TAR_ID,
                    Tar_NOM = Tarea.TAR_NOM,
                    Tar_DES = Tarea.TAR_DES,
                    Tar_EST = Tarea.TAR_EST,
                    Pri_ID = Tarea.PRI_ID != Guid.Empty ? Tarea.PRI_ID : null,
                    Proc_ID = Tarea.PROC_ID,
                    Tar_FEC_INI = Tarea.TAR_FEC_INI,
                    Tar_FEC_CRE = Tarea.TAR_FEC_CRE,
                    Tar_FEC_MOD = DateTimeOffset.Now
                };

                var result = await Client.UpdateTarea.ExecuteAsync(input);

                if (result.Errors != null && result.Errors.Any())
                    Console.WriteLine($"Error guardado: {result.Errors.First().Message}");
                else
                    Console.WriteLine("✅ Tarea guardada automáticamente al cerrar");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar: {ex.Message}");
            }
        }

        private async Task Cerrar()
        {
            await GuardarTarea();
            await OnClose.InvokeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await GuardarTarea();
        }

        private void ManejarCambioBitacora(ChangeEventArgs e)
        {
            var val = e.Value?.ToString();
            if (string.IsNullOrEmpty(val)) return;
            TipoModal = val == "BIT_SOL" ? "HISTORIAL_SOLUCIONES" : "HISTORIAL_FRICCIONES";
            MostrarModalCrear = true;
            StateHasChanged();
        }

        private void AbrirModalEditarSolucion(SolucionesModel sol)
        {
            SolucionSeleccionada = sol;
            TipoModal = "SOLUCION_EDITAR";
            MostrarModalCrear = true;
            StateHasChanged();
        }

        private void AbrirModalEditarFriccion(FriccionModel fri)
        {
            FriccionSeleccionada = fri;
            TipoModal = "FRICCION_EDITAR";
            MostrarModalCrear = true;
            StateHasChanged();
        }

        private async Task AlCrearExitoso()
        {
            MostrarModalCrear = false;
            await CargarDatosTarea();
            StateHasChanged();
        }

        private void AbrirModalCrearFriccion() { TipoModal = "FRICCION_NUEVA"; MostrarModalCrear = true; }
        private void AbrirModalCrearSolucion() { TipoModal = "SOLUCION_NUEVA"; MostrarModalCrear = true; }
        private void CerrarModalInterno() => MostrarModalCrear = false;

        private async Task EliminarSolucion(Guid id)
        {
            try
            {
                var result = await Client.DeleteSolucion.ExecuteAsync(id);
                if (result.Errors.Any()) { Console.WriteLine($"Error: {result.Errors.First().Message}"); return; }
                await CargarDatosTarea();
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        private async Task EliminarFriccion(Guid id)
        {
            try
            {
                await Client.DeleteFriccion.ExecuteAsync(id);
                await CargarDatosTarea();
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }
    }
}
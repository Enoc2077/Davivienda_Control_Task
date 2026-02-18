using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class DetalleTarea : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public TareaModel? Tarea { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private List<FriccionModel> FriccionesList = new();
        private List<SolucionesModel> SolucionesList = new();
        private string NombreProceso = "CARGANDO...";
        private string NombreProyecto = "CARGANDO...";
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
            "HISTORIAL_SOLUCIONES" => "Bitácora Global de Soluciones",
            "HISTORIAL_FRICCIONES" => "Bitácora Global de Fricciones",
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
                            NombreProyecto = resProy.Data?.ProyectoById?.Pro_NOM.ToUpper() ?? "PROYECTO NO ENCONTRADO";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Jerarquía: {ex.Message}");
                NombreProceso = "ERROR"; NombreProyecto = "ERROR";
            }
            StateHasChanged();
        }

        private async Task CargarDatosTarea()
        {
            try
            {
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

                var resSol = await Client.GetSoluciones.ExecuteAsync();
                var idsFricciones = FriccionesList.Select(f => f.FRI_ID).ToList();
                SolucionesList = resSol.Data?.Soluciones?
                    .Where(s => s.Fri_ID.HasValue && idsFricciones.Contains(s.Fri_ID.Value))
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
            catch (Exception ex) { Console.WriteLine($"Error Carga: {ex.Message}"); }
            StateHasChanged();
        }

        private async Task GuardarEstadoTarea()
        {
            if (Tarea == null) return;
            var input = new TareaModelInput { Tar_ID = Tarea.TAR_ID, Tar_NOM = Tarea.TAR_NOM, Tar_EST = Tarea.TAR_EST, Proc_ID = Tarea.PROC_ID ?? Guid.Empty, Pri_ID = Tarea.PRI_ID ?? Guid.Empty };
            await Client.UpdateTarea.ExecuteAsync(input);
            StateHasChanged();
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
                if (result.Errors.Any()) { Console.WriteLine($"Error FK: {result.Errors.First().Message}"); return; }
                await CargarDatosTarea();
            }
            catch (Exception ex) { Console.WriteLine($"Error Delete: {ex.Message}"); }
        }

        private async Task EliminarFriccion(Guid id)
        {
            try
            {
                await Client.DeleteFriccion.ExecuteAsync(id);
                await CargarDatosTarea();
            }
            catch (Exception ex) { Console.WriteLine($"Error Delete: {ex.Message}"); }
        }

        private async Task Cerrar() => await OnClose.InvokeAsync();
    }
}
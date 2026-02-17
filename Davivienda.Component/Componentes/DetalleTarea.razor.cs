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
        private string NombreProceso = "Cargando...";
        private string NombreProyecto = "Cargando...";
        private bool MostrarModalCrear = false;
        private string TipoModal = "";

        private string TituloModal => TipoModal switch
        {
            "FRICCION_NUEVA" => "Nueva Fricción",
            "SOLUCION_NUEVA" => "Nueva Solución",
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
                        NombreProceso = resProc.Data.ProcesoById.Proc_NOM;
                        var proId = resProc.Data.ProcesoById.Pro_ID;
                        if (proId.HasValue)
                        {
                            var resProy = await Client.GetProyectoById.ExecuteAsync(proId.Value);
                            NombreProyecto = resProy.Data?.ProyectoById?.Pro_NOM ?? "Desconocido";
                        }
                    }
                }
            }
            catch { NombreProceso = "Error"; NombreProyecto = "Error"; }
            StateHasChanged();
        }

        private async Task CargarDatosTarea()
        {
            try
            {
                var resFri = await Client.GetFricciones.ExecuteAsync();
                FriccionesList = resFri.Data?.Fricciones?.Where(f => f.Tar_ID == Tarea?.TAR_ID).Select(f => new FriccionModel { FRI_ID = f.Fri_ID, FRI_TIP = f.Fri_TIP }).ToList() ?? new();

                var resSol = await Client.GetSoluciones.ExecuteAsync();
                var idsFricciones = FriccionesList.Select(f => f.FRI_ID).ToList();
                SolucionesList = resSol.Data?.Soluciones?.Where(s => s.Fri_ID.HasValue && idsFricciones.Contains(s.Fri_ID.Value)).Select(s => new SolucionesModel { SOL_ID = s.Sol_ID, SOL_NOM = s.Sol_NOM }).ToList() ?? new();
            }
            catch { }
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
            StateHasChanged(); // Forzar la aparición del modal
        }

        // Dentro de ManejarCambioBitacora, forzamos el renderizado


        // Asegurarse de que al crear algo exitoso se recarguen las listas
        private async Task AlCrearExitoso()
        {
            MostrarModalCrear = false;
            await CargarDatosTarea(); // Recarga la info de la DB para ver la nueva solución/fricción
            StateHasChanged();
        }

        private void AbrirModalCrearFriccion() { TipoModal = "FRICCION_NUEVA"; MostrarModalCrear = true; }
        private void AbrirModalCrearSolucion() { TipoModal = "SOLUCION_NUEVA"; MostrarModalCrear = true; }
        private void AbrirModalEditarSolucion(SolucionesModel sol) { TipoModal = "SOLUCION_EDITAR"; MostrarModalCrear = true; }
        private void AbrirModalEditarFriccion(FriccionModel fri) { TipoModal = "FRICCION_EDITAR"; MostrarModalCrear = true; }
        private void CerrarModalInterno() => MostrarModalCrear = false;
        private async Task EliminarSolucion(Guid id) { await Client.DeleteSolucion.ExecuteAsync(id); await CargarDatosTarea(); }
        private async Task EliminarFriccion(Guid id) { await Client.DeleteFriccion.ExecuteAsync(id); await CargarDatosTarea(); }
        private async Task Cerrar() => await OnClose.InvokeAsync();
    }
}
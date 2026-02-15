using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class DetalleTarea
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public TareaModel? Tarea { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private List<FriccionModel> FriccionesList = new();
        private List<SolucionesModel> SolucionesList = new();
        private bool CargandoDatos = true;
        private bool MostrarModalCrear = false;
        private string TipoModal = "";

        private FriccionModel? FriccionSeleccionada;
        private SolucionesModel? SolucionSeleccionada;

        private string TituloModal => TipoModal switch
        {
            "FRICCION_NUEVA" => "Nueva Fricción Detectada",
            "FRICCION_EDITAR" => "Editar Información de Fricción",
            "SOLUCION_NUEVA" => "Nueva Solución Propuesta",
            "SOLUCION_EDITAR" => "Editar Solución y Bitácora",
            "HISTORIAL_SOLUCIONES" => "Historial Global: Bitácora de Soluciones",
            "HISTORIAL_FRICCIONES" => "Historial Global: Bitácora de Fricciones",
            _ => "Detalle"
        };

        protected override async Task OnInitializedAsync()
        {
            // Usamos el flag CargandoDatos para evitar advertencias de compilación
            if (Tarea != null) { await CargarDatosTarea(); }
            else { CargandoDatos = false; }
        }

        private async Task CargarDatosTarea()
        {
            CargandoDatos = true;
            try
            {
                var resFri = await Client.GetFricciones.ExecuteAsync();
                if (resFri.Data?.Fricciones != null)
                {
                    FriccionesList = resFri.Data.Fricciones
                        .Where(f => f.Tar_ID == Tarea?.TAR_ID)
                        .Select(f => new FriccionModel
                        {
                            FRI_ID = f.Fri_ID,
                            FRI_TIP = f.Fri_TIP,
                            FRI_DES = f.Fri_DES,
                            FRI_EST = f.Fri_EST,
                            FRI_IMP = f.Fri_IMP,
                            FRI_FEC_CRE = f.Fri_FEC_CRE.DateTime
                        }).ToList();
                }

                var resSol = await Client.GetSoluciones.ExecuteAsync();
                if (resSol.Data?.Soluciones != null)
                {
                    // Filtramos soluciones que pertenezcan a las fricciones de esta tarea
                    var idsFricciones = FriccionesList.Select(f => f.FRI_ID).ToList();
                    SolucionesList = resSol.Data.Soluciones
                        .Where(s => s.Fri_ID.HasValue && idsFricciones.Contains(s.Fri_ID.Value))
                        .Select(s => new SolucionesModel
                        {
                            SOL_ID = s.Sol_ID,
                            SOL_NOM = s.Sol_NOM,
                            SOL_DES = s.Sol_DES,
                            SOL_EST = s.Sol_EST,
                            SOL_TIE_RES = s.Sol_TIE_RES,
                            SOL_NIV_EFE = s.Sol_NIV_EFE,
                            SOL_FEC_CRE = s.Sol_FEC_CRE.DateTime
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DetalleTarea: {ex.Message}");
            }
            finally
            {
                CargandoDatos = false;
                StateHasChanged();
            }
        }

        private void ManejarCambioBitacora(ChangeEventArgs e)
        {
            var seleccion = e.Value?.ToString();
            if (string.IsNullOrEmpty(seleccion)) return;

            if (seleccion == "BIT_SOL")
            {
                TipoModal = "HISTORIAL_SOLUCIONES";
                MostrarModalCrear = true;
            }
            else if (seleccion == "BIT_FRI")
            {
                TipoModal = "HISTORIAL_FRICCIONES";
                MostrarModalCrear = true;
            }
        }

        private async Task EliminarSolucion(Guid id) { await Client.DeleteSolucion.ExecuteAsync(id); await CargarDatosTarea(); }
        private async Task EliminarFriccion(Guid id) { await Client.DeleteFriccion.ExecuteAsync(id); await CargarDatosTarea(); }

        private void AbrirModalCrearFriccion() { FriccionSeleccionada = null; TipoModal = "FRICCION_NUEVA"; MostrarModalCrear = true; }
        private void AbrirModalEditarFriccion(FriccionModel friccion) { FriccionSeleccionada = friccion; TipoModal = "FRICCION_EDITAR"; MostrarModalCrear = true; }
        private void AbrirModalCrearSolucion() { SolucionSeleccionada = null; TipoModal = "SOLUCION_NUEVA"; MostrarModalCrear = true; }
        private void AbrirModalEditarSolucion(SolucionesModel sol) { SolucionSeleccionada = sol; TipoModal = "SOLUCION_EDITAR"; MostrarModalCrear = true; }

        private void CerrarModalInterno()
        {
            MostrarModalCrear = false;
            // Limpiamos selecciones al cerrar
            FriccionSeleccionada = null;
            SolucionSeleccionada = null;
        }

        private async Task AlCrearExitoso()
        {
            MostrarModalCrear = false;
            await CargarDatosTarea();
        }

        private async Task Cerrar() => await OnClose.InvokeAsync();
    }
}
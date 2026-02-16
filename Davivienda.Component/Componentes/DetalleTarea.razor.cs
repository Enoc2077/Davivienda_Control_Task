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

        private string NombreProceso = "Cargando...";
        private string NombreProyecto = "Cargando...";

        private bool CargandoDatos = true;
        private bool CargandoJerarquia = true;
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
            if (Tarea != null)
            {
                await CargarDatosTarea();
                await CargarJerarquia();
            }
            else
            {
                CargandoDatos = false;
                CargandoJerarquia = false;
            }
        }

        private async Task CargarJerarquia()
        {
            CargandoJerarquia = true;
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
                            NombreProyecto = resProy.Data?.ProyectoById?.Pro_NOM ?? "Proyecto Desconocido";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Jerarquía: {ex.Message}");
                NombreProceso = "No disponible";
                NombreProyecto = "No disponible";
            }
            finally
            {
                CargandoJerarquia = false;
                StateHasChanged();
            }
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
                    var idsFricciones = FriccionesList.Select(f => f.FRI_ID).ToList();
                    SolucionesList = resSol.Data.Soluciones
                        .Where(s => s.Fri_ID.HasValue && idsFricciones.Contains(s.Fri_ID.Value))
                        .Select(s => new SolucionesModel
                        {
                            SOL_ID = s.Sol_ID,
                            SOL_NOM = s.Sol_NOM,
                            SOL_DES = s.Sol_DES,
                            SOL_EST = s.Sol_EST,
                            SOL_FEC_CRE = s.Sol_FEC_CRE.DateTime
                        }).ToList();
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error Datos: {ex.Message}"); }
            finally { CargandoDatos = false; StateHasChanged(); }
        }

        // --- MÉTODO DE GUARDADO CORREGIDO ---
        private async Task GuardarEstadoTarea()
        {
            if (Tarea == null) return;

            try
            {
                // Validamos que la prioridad no sea nula antes de enviar para evitar el error de FK
                if (Tarea.PRI_ID == null || Tarea.PRI_ID == Guid.Empty)
                {
                    Console.WriteLine("Error local: No se puede actualizar una tarea sin una prioridad válida.");
                    return;
                }

                var input = new TareaModelInput
                {
                    Tar_ID = Tarea.TAR_ID,
                    Tar_NOM = Tarea.TAR_NOM,
                    Tar_DES = Tarea.TAR_DES ?? "Sin descripción",
                    Tar_EST = Tarea.TAR_EST,
                    Tar_FEC_INI = Tarea.TAR_FEC_INI,
                    Tar_FEC_FIN = Tarea.TAR_FEC_FIN,

                    // Usamos .Value para asegurar que enviamos el Guid real y no un nulo
                    Proc_ID = Tarea.PROC_ID ?? Guid.Empty,
                    Pri_ID = Tarea.PRI_ID.Value, // Obligatorio por el error de FK recibido
                    Usu_ID = Tarea.USU_ID ?? Guid.Empty,

                    Tar_FEC_CRE = Tarea.TAR_FEC_CRE,
                    Tar_FEC_MOD = DateTimeOffset.Now
                };

                var result = await Client.UpdateTarea.ExecuteAsync(input);

                if (result.Errors.Any())
                {
                    foreach (var err in result.Errors)
                    {
                        // Aquí verás el error de FK si el ID enviado sigue siendo incorrecto
                        Console.WriteLine($"Error de Base de Datos: {err.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("¡Estado actualizado con éxito!");
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de ejecución: {ex.Message}");
            }
        }

        private void ManejarCambioBitacora(ChangeEventArgs e)
        {
            var seleccion = e.Value?.ToString();
            if (string.IsNullOrEmpty(seleccion)) return;
            TipoModal = seleccion == "BIT_SOL" ? "HISTORIAL_SOLUCIONES" : "HISTORIAL_FRICCIONES";
            MostrarModalCrear = true;
        }

        private async Task EliminarSolucion(Guid id) { await Client.DeleteSolucion.ExecuteAsync(id); await CargarDatosTarea(); }
        private async Task EliminarFriccion(Guid id) { await Client.DeleteFriccion.ExecuteAsync(id); await CargarDatosTarea(); }

        private void AbrirModalCrearFriccion() { FriccionSeleccionada = null; TipoModal = "FRICCION_NUEVA"; MostrarModalCrear = true; }
        private void AbrirModalEditarFriccion(FriccionModel friccion)
        {
            FriccionSeleccionada = friccion;
            TipoModal = "FRICCION_EDITAR";
            MostrarModalCrear = true;
        }
        private void AbrirModalCrearSolucion() { SolucionSeleccionada = null; TipoModal = "SOLUCION_NUEVA"; MostrarModalCrear = true; }
        private void AbrirModalEditarSolucion(SolucionesModel sol)
        {
            SolucionSeleccionada = sol;
            TipoModal = "SOLUCION_EDITAR";
            MostrarModalCrear = true;
        }

        private void CerrarModalInterno() { MostrarModalCrear = false; }
        private async Task AlCrearExitoso() { MostrarModalCrear = false; await CargarDatosTarea(); }
        private async Task Cerrar() => await OnClose.InvokeAsync();
    }
}
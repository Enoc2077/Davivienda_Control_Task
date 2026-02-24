using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class BitacoraSolucion : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }

        private List<SolucionesModel> SolucionesGlobales = new();
        private List<SolucionesModel> SolucionesFiltradas = new();

        private List<AreasModel> AreasList = new();
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosList = new();
        private List<TareaModel> TareasList = new();
        private List<PrioridadModel> PrioridadesList = new();

        public SolucionesModel? SolucionSeleccionada { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CargarTodo();
        }

        private async Task CargarTodo()
        {
            try
            {
                // 1. Cargamos las Soluciones (Lo que se ve en la lista de la izquierda)
                var resSol = await Client.GetSoluciones.ExecuteAsync();
                SolucionesGlobales = resSol.Data?.Soluciones?.Select(s => new SolucionesModel
                {
                    SOL_ID = s.Sol_ID,
                    SOL_NOM = s.Sol_NOM,
                    FRI_ID = s.Fri_ID // Este es el ID de la Tarea/Fricción vinculada
                                      // ... resto de campos
                }).ToList() ?? new();

                // 2. Cargamos las Tareas REALES (Lo que aparecerá en el dropdown de filtros)
                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasList = resTar.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    PROC_ID = t.Proc_ID
                }).ToList() ?? new();

                SolucionesFiltradas = SolucionesGlobales;
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            Console.WriteLine($"\n[DEBUG BITÁCORA] === INICIO DE FILTRADO ELECTRÓNICO ===");
            Console.WriteLine($"[DEBUG] Total Soluciones en Memoria: {SolucionesGlobales.Count}");

            if (filtros == null || !filtros.Any())
            {
                Console.WriteLine("[DEBUG] Sin filtros activos. Mostrando todo.");
                SolucionesFiltradas = SolucionesGlobales.ToList();
            }
            else
            {
                var resultado = SolucionesGlobales.AsEnumerable();

                foreach (var f in filtros)
                {
                    Console.WriteLine($"[DEBUG] Aplicando Filtro: {f.Tipo} | Etiqueta: {f.Etiqueta} | ID: {f.Id}");

                    switch (f.Tipo)
                    {
                        case "Nombre":
                            resultado = resultado.Where(s => s.SOL_NOM.Contains(f.Etiqueta, StringComparison.OrdinalIgnoreCase));
                            Console.WriteLine($"   -> Match por Nombre. Quedan: {resultado.Count()}");
                            break;

                        case "Tarea":
                            // Filtramos las SOLUCIONES cuyo vínculo (FRI_ID) sea igual a la TAREA seleccionada (f.Id)
                            resultado = resultado.Where(s => s.FRI_ID.HasValue && s.FRI_ID.Value == f.Id);
                            Console.WriteLine($" -> Match por Tarea. Quedan: {resultado.Count()} soluciones.");
                            break;

                        case "Proyecto":
                            // PASO 1: Procesos del Proyecto
                            var procIds = ProcesosList.Where(p => p.PRO_ID == f.Id).Select(p => p.PROC_ID).ToList();
                            Console.WriteLine($"   [PASO 1] Procesos encontrados para el Proyecto: {procIds.Count}");

                            // PASO 2: Tareas de esos Procesos
                            var tarIds = TareasList.Where(t => t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value)).Select(t => t.TAR_ID).ToList();
                            Console.WriteLine($"   [PASO 2] Tareas vinculadas a esos procesos: {tarIds.Count}");

                            // PASO 3: Filtrar Soluciones
                            resultado = resultado.Where(s => s.FRI_ID.HasValue && tarIds.Contains(s.FRI_ID.Value));
                            Console.WriteLine($"   -> Match por Proyecto Final. Quedan: {resultado.Count()}");
                            break;

                        case "Area":
                            // PASO 1: Proyectos del Área
                            var proyIds = ProyectosList.Where(p => p.ARE_ID == f.Id).Select(p => p.PRO_ID).ToList();
                            Console.WriteLine($"   [PASO 1] Proyectos en el Área: {proyIds.Count}");

                            // PASO 2: Procesos
                            var prcIds = ProcesosList.Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToList();
                            Console.WriteLine($"   [PASO 2] Procesos en esos proyectos: {prcIds.Count}");

                            // PASO 3: Tareas
                            var tIds = TareasList.Where(t => t.PROC_ID.HasValue && prcIds.Contains(t.PROC_ID.Value)).Select(t => t.TAR_ID).ToList();
                            Console.WriteLine($"   [PASO 3] Tareas finales encontradas: {tIds.Count}");

                            // PASO 4: Filtrar Soluciones
                            resultado = resultado.Where(s => s.FRI_ID.HasValue && tIds.Contains(s.FRI_ID.Value));
                            Console.WriteLine($"   -> Match por Área Final. Quedan: {resultado.Count()}");
                            break;

                        case "Prioridad":
                            resultado = resultado.Where(s => s.SOL_EST.Equals(f.Etiqueta, StringComparison.OrdinalIgnoreCase));
                            Console.WriteLine($"   -> Match por Prioridad/Estado. Quedan: {resultado.Count()}");
                            break;
                    }

                    if (!resultado.Any())
                    {
                        Console.WriteLine($"[ALERTA] El filtro '{f.Tipo}' vació la lista por completo. Revisa si las relaciones en la DB existen.");
                        break; // Si ya dio 0, no hace falta seguir con los demás filtros del foreach
                    }
                }
                SolucionesFiltradas = resultado.ToList();
            }

            Console.WriteLine($"[DEBUG] === FILTRADO FINALIZADO: {SolucionesFiltradas.Count} resultados mostrados ===\n");
            StateHasChanged();
        }

        private void SeleccionarSolucion(SolucionesModel sol) { SolucionSeleccionada = sol; StateHasChanged(); }

        private async Task Regresar() => await OnClose.InvokeAsync();

        private TareaModel ConvertirASolucionModel(SolucionesModel sol)
        {
            return new TareaModel
            {
                TAR_ID = sol.SOL_ID,
                TAR_NOM = sol.SOL_NOM,
                TAR_DES = sol.SOL_DES,
                TAR_EST = sol.SOL_EST,
                TAR_FEC_INI = sol.SOL_FEC_CRE,
                USU_ID = sol.USU_ID
            };
        }
    }
}
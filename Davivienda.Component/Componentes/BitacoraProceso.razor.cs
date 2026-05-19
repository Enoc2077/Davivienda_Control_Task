using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class BitacoraProceso : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public Guid ProyectoId { get; set; }

        private List<ProcesoModel> ProcesosGlobales = new();
        private List<ProcesoModel> ProcesosFiltrados = new();
        private List<ProyectosModel> ProyectosGlobales = new();
        private List<TareaModel> TareasGlobales = new();
        private List<TareaModel> TareasDeProceso = new();

        private List<AreasModel> AreasList = new();
        private List<ProyectosModel> ProyectosList = new();

        public ProcesoModel? ProcesoSeleccionado { get; set; }

        // ── Controla si se muestra el componente Procesos
        private bool MostrarProcesos = false;

        // ── Proyecto al que pertenece el proceso seleccionado
        // (puede ser diferente al ProyectoId del parámetro)
        private ProyectosModel? ProyectoParaProcesos;

        private string UserRole = "";
        private Guid? UserAreaId;

        protected override async Task OnInitializedAsync()
        {
            await CargarUsuarioYRol();
            await CargarTodo();
        }

        // ── ROL Y AREA ──────────────────────────────────────────
        private async Task CargarUsuarioYRol()
        {
            try
            {
                var authState = await AuthProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                UserRole = user.FindFirst(ClaimTypes.Role)?.Value
                           ?? user.FindFirst("role")?.Value
                           ?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                           ?? "Empleado";

                var usuNum = user.FindFirst("USU_NUM")?.Value ?? "";
                if (string.IsNullOrEmpty(usuNum)) return;

                var resU = await Client.GetUsuarios.ExecuteAsync();
                var u = resU.Data?.Usuarios.FirstOrDefault(x => x.Usu_NUM == usuNum);
                if (u != null) UserAreaId = u.Are_ID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CargarUsuarioYRol: {ex.Message}");
            }
        }

        // ── CARGA ────────────────────────────────────────────────
        private async Task CargarTodo()
        {
            try
            {
                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosGlobales = resProy.Data?.Proyectos.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    PRO_EST = p.Pro_EST,
                    ARE_ID = p.Are_ID
                }).ToList() ?? new();

                var resProc = await Client.GetProcesos.ExecuteAsync();
                var todosProcesos = resProc.Data?.Procesos.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PROC_DES = p.Proc_DES,
                    PROC_FRE = p.Proc_FRE ?? "Sin frecuencia",
                    PROC_EST = p.Proc_EST,
                    PRO_ID = p.Pro_ID,
                    PROC_FEC_CRE = p.Proc_FEC_CRE.DateTime,
                    PROC_FEC_MOD = p.Proc_FEC_MOD?.DateTime
                }).ToList() ?? new();

                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasGlobales = resTar.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    TAR_EST = t.Tar_EST,
                    PROC_ID = t.Proc_ID
                }).ToList() ?? new();

                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();

                // Filtrar listas por rol
                ProyectosList = FiltrarProyectosPorRol(ProyectosGlobales);
                if (!EsGerente(UserRole) && UserAreaId.HasValue)
                    AreasList = AreasList.Where(a => a.ARE_ID == UserAreaId).ToList();

                // Aplicar lógica bitácora + filtro por rol
                var enBitacora = AplicarLogicaBitacora(todosProcesos);
                ProcesosGlobales = FiltrarProcesosPorRol(enBitacora);

                ProcesosFiltrados = ProcesosGlobales
                    .OrderByDescending(p => p.PROC_FEC_CRE)
                    .ToList();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CargarTodo: {ex.Message}");
            }
        }

        // ── LÓGICA BITÁCORA ─────────────────────────────────────
        private List<ProcesoModel> AplicarLogicaBitacora(List<ProcesoModel> todos)
        {
            var enBitacora = new List<ProcesoModel>();
            var porProyecto = todos.GroupBy(p => p.PRO_ID);

            foreach (var grupo in porProyecto)
            {
                if (!grupo.Key.HasValue) continue;
                var proyecto = ProyectosGlobales.FirstOrDefault(p => p.PRO_ID == grupo.Key.Value);
                if (proyecto == null) continue;

                var procesosDelProyecto = grupo.ToList();

                bool proyectoFinalizado =
                    proyecto.PRO_EST?.Equals("FINALIZADO", StringComparison.OrdinalIgnoreCase) == true ||
                    proyecto.PRO_EST?.Equals("Finalizado", StringComparison.OrdinalIgnoreCase) == true ||
                    proyecto.PRO_EST?.Equals("Completado", StringComparison.OrdinalIgnoreCase) == true;

                if (proyectoFinalizado)
                {
                    // Proyecto finalizado → TODOS sus procesos van a bitácora y se quedan
                    enBitacora.AddRange(procesosDelProyecto);
                }
                else
                {
                    // Proyecto activo → solo inactivos excedentes de 5
                    var inactivos = procesosDelProyecto
                        .Where(p => p.PROC_EST == false)
                        .OrderByDescending(p => p.PROC_FEC_CRE)
                        .ToList();

                    if (inactivos.Count > 5)
                        enBitacora.AddRange(inactivos.Skip(5));
                }
            }

            return enBitacora;
        }

        // ── FILTRADO POR ROL ────────────────────────────────────
        private List<ProcesoModel> FiltrarProcesosPorRol(List<ProcesoModel> todos)
        {
            if (EsGerente(UserRole)) return todos;
            if (!UserAreaId.HasValue) return new();

            var proyDelArea = ProyectosGlobales
                .Where(p => p.ARE_ID == UserAreaId)
                .Select(p => p.PRO_ID)
                .ToHashSet();

            return todos.Where(p => p.PRO_ID.HasValue && proyDelArea.Contains(p.PRO_ID.Value)).ToList();
        }

        private List<ProyectosModel> FiltrarProyectosPorRol(List<ProyectosModel> todos)
        {
            if (EsGerente(UserRole)) return todos;
            if (!UserAreaId.HasValue) return new();
            return todos.Where(p => p.ARE_ID == UserAreaId).ToList();
        }

        // ── FILTROS ─────────────────────────────────────────────
        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            if (filtros == null || !filtros.Any())
            {
                ProcesosFiltrados = ProcesosGlobales.OrderByDescending(p => p.PROC_FEC_CRE).ToList();
                StateHasChanged();
                return;
            }

            var resultado = ProcesosGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(p => p.PROC_NOM.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                    case "Año":
                        if (int.TryParse(filtro.Etiqueta, out int anio))
                            resultado = resultado.Where(p => p.PROC_FEC_CRE.Year == anio);
                        break;
                    case "Mes":
                        var meses = System.Globalization.CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.MonthNames;
                        int numMes = Array.FindIndex(meses, m => m.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase)) + 1;
                        if (numMes > 0) resultado = resultado.Where(p => p.PROC_FEC_CRE.Month == numMes);
                        break;
                    case "Dia":
                        if (int.TryParse(filtro.Etiqueta, out int dia))
                            resultado = resultado.Where(p => p.PROC_FEC_CRE.Day == dia);
                        break;
                    case "Proceso":
                        resultado = resultado.Where(p => p.PROC_ID == filtro.Id);
                        break;
                    case "Proyecto":
                        resultado = resultado.Where(p => p.PRO_ID == filtro.Id);
                        break;
                    case "Area":
                        var proyIds = ProyectosGlobales
                            .Where(p => p.ARE_ID == filtro.Id)
                            .Select(p => p.PRO_ID).ToList();
                        resultado = resultado.Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value));
                        break;
                }
            }

            ProcesosFiltrados = resultado.OrderByDescending(p => p.PROC_FEC_CRE).ToList();
            StateHasChanged();
        }

        // ── DETALLE ─────────────────────────────────────────────
        private void AbrirDetalleProceso(ProcesoModel proceso)
        {
            ProcesoSeleccionado = proceso;
            MostrarProcesos = false;
            TareasDeProceso = TareasGlobales
                .Where(t => t.PROC_ID == proceso.PROC_ID)
                .ToList();
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            ProcesoSeleccionado = null;
            TareasDeProceso = new();
            StateHasChanged();
        }

        // ── VER PROCESOS DEL PROYECTO ────────────────────────────
        // Toma el PRO_ID del PROCESO seleccionado (no del parámetro ProyectoId).
        // Ej: entré desde Proyecto 1, seleccioné un proceso del Proyecto 2
        // → abre Procesos con los datos del Proyecto 2.
        private void AbrirProcesosDelProyecto()
        {
            if (ProcesoSeleccionado?.PRO_ID == null) return;

            // Buscar el proyecto al que pertenece ESTE proceso específico
            ProyectoParaProcesos = ProyectosGlobales
                .FirstOrDefault(p => p.PRO_ID == ProcesoSeleccionado.PRO_ID.Value);

            if (ProyectoParaProcesos == null)
            {
                Console.WriteLine($"⚠️ Proyecto no encontrado para PRO_ID: {ProcesoSeleccionado.PRO_ID}");
                return;
            }

            Console.WriteLine($"✅ Abriendo Procesos del proyecto: {ProyectoParaProcesos.PRO_NOM}");

            // Cerrar el detalle del proceso y abrir el componente Procesos
            ProcesoSeleccionado = null;
            TareasDeProceso = new();
            MostrarProcesos = true;
            StateHasChanged();
        }

        private void CerrarProcesos()
        {
            // Al volver desde Procesos regresamos a la bitácora
            MostrarProcesos = false;
            ProyectoParaProcesos = null;
            StateHasChanged();
        }

        // ── HELPERS ─────────────────────────────────────────────
        private string ObtenerNombreProyecto(Guid? id)
        {
            if (!id.HasValue) return "Sin proyecto";
            return ProyectosGlobales.FirstOrDefault(p => p.PRO_ID == id.Value)?.PRO_NOM ?? "Proyecto no encontrado";
        }

        private int ObtenerCantidadTareas(Guid procesoId) =>
            TareasGlobales.Count(t => t.PROC_ID == procesoId);

        private string ObtenerClaseTarea(string? estado) => estado switch
        {
            "Completado" => "tarea-completada",
            "En Progreso" => "tarea-progreso",
            "Pendiente" => "tarea-pendiente",
            _ => "tarea-pendiente"
        };

        private bool EsGerente(string rol)
        {
            var roles = new[] { "Gerente", "Administrador", "Enoc", "Admin" };
            return roles.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        private async Task Regresar() => await OnClose.InvokeAsync();
    }
}
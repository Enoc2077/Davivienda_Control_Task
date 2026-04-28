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
    public partial class BitacoraTarea : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }

        private List<TareaModel> TareasGlobales = new();
        private List<TareaModel> TareasFiltradas = new();
        private List<ProcesoModel> ProcesosGlobales = new();
        private List<ProyectosModel> ProyectosGlobales = new();

        private List<AreasModel> AreasList = new();
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosList = new();
        private List<PrioridadModel> PrioridadesList = new();

        public TareaModel? TareaSeleccionada { get; set; }

        // Tarea que se abre en DetalleTarea completo
        private TareaModel? TareaParaDetalle;

        // Rol y area
        private string UserRole = "";
        private Guid? UserAreaId;
        private UsuarioModel? UsuarioActual;

        private bool PuedeVerTarea => EsGerente(UserRole) || EsLiderTecnico(UserRole);

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
                if (u == null) return;

                UsuarioActual = new UsuarioModel
                {
                    USU_ID = u.Usu_ID,
                    USU_NOM = u.Usu_NOM,
                    USU_NUM = u.Usu_NUM,
                    ARE_ID = u.Are_ID,
                    ROL_ID = u.Rol_ID
                };
                UserAreaId = UsuarioActual.ARE_ID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CargarUsuarioYRol: {ex.Message}");
            }
        }

        // ── CARGA PRINCIPAL ─────────────────────────────────────
        private async Task CargarTodo()
        {
            try
            {
                // 1. Procesos
                var resProc = await Client.GetProcesos.ExecuteAsync();
                ProcesosGlobales = resProc.Data?.Procesos?.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PROC_DES = p.Proc_DES,
                    PROC_EST = p.Proc_EST,
                    PRO_ID = p.Pro_ID
                }).ToList() ?? new();

                // 2. Proyectos
                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosGlobales = resProy.Data?.Proyectos.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    ARE_ID = p.Are_ID
                }).ToList() ?? new();

                // 3. Areas
                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();

                // 4. Prioridades
                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                PrioridadesList = resPrio.Data?.Prioridades.Select(p => new PrioridadModel
                {
                    PRI_ID = p.Pri_ID,
                    PRI_NOM = p.Pri_NOM
                }).ToList() ?? new();

                // 5. Tareas — solo Completadas con proceso inactivo
                var resTar = await Client.GetTareas.ExecuteAsync();
                var todas = resTar.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    TAR_DES = t.Tar_DES,
                    TAR_EST = t.Tar_EST,
                    TAR_FEC_INI = t.Tar_FEC_INI.DateTime,
                    TAR_FEC_FIN = t.Tar_FEC_FIN?.DateTime,
                    PROC_ID = t.Proc_ID,
                    PRI_ID = t.Pri_ID,
                    USU_ID = t.Usu_ID,
                    TAR_FEC_CRE = t.Tar_FEC_CRE.DateTime,
                    TAR_FEC_MOD = t.Tar_FEC_MOD
                }).ToList() ?? new();

                TareasGlobales = todas.Where(t =>
                {
                    if (t.TAR_EST != "Completado") return false;
                    if (!t.PROC_ID.HasValue) return false;
                    var proc = ProcesosGlobales.FirstOrDefault(p => p.PROC_ID == t.PROC_ID.Value);
                    return proc?.PROC_EST == false;
                }).ToList();

                // 6. Filtrar por rol y area
                TareasGlobales = FiltrarTareasPorRol(TareasGlobales);
                FiltrarListasPorRol();

                // Para el componente Filtros
                ProyectosList = ProyectosGlobales;
                ProcesosList = ProcesosGlobales;

                TareasFiltradas = TareasGlobales.OrderByDescending(t => t.TAR_FEC_CRE).ToList();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CargarTodo: {ex.Message}");
            }
        }

        // ── FILTRADO POR ROL ────────────────────────────────────
        private List<TareaModel> FiltrarTareasPorRol(List<TareaModel> todas)
        {
            if (EsGerente(UserRole)) return todas;
            if (!UserAreaId.HasValue) return new();

            var proyDelArea = ProyectosGlobales.Where(p => p.ARE_ID == UserAreaId).Select(p => p.PRO_ID).ToHashSet();
            var procDelArea = ProcesosGlobales.Where(p => p.PRO_ID.HasValue && proyDelArea.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToHashSet();

            return todas.Where(t => t.PROC_ID.HasValue && procDelArea.Contains(t.PROC_ID.Value)).ToList();
        }

        private void FiltrarListasPorRol()
        {
            if (EsGerente(UserRole)) return;
            if (!UserAreaId.HasValue) { AreasList = new(); ProyectosList = new(); ProcesosList = new(); return; }

            AreasList = AreasList.Where(a => a.ARE_ID == UserAreaId).ToList();
            var proyIds = ProyectosGlobales.Where(p => p.ARE_ID == UserAreaId).Select(p => p.PRO_ID).ToHashSet();
            ProyectosList = ProyectosGlobales.Where(p => proyIds.Contains(p.PRO_ID)).ToList();
            var procIds = ProcesosGlobales.Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToHashSet();
            ProcesosList = ProcesosGlobales.Where(p => procIds.Contains(p.PROC_ID)).ToList();
        }

        // ── FILTROS ─────────────────────────────────────────────
        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            if (filtros == null || !filtros.Any())
            {
                TareasFiltradas = TareasGlobales.OrderByDescending(t => t.TAR_FEC_CRE).ToList();
                StateHasChanged();
                return;
            }

            var resultado = TareasGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(t => t.TAR_NOM.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                    case "Año":
                        if (int.TryParse(filtro.Etiqueta, out int anio))
                            resultado = resultado.Where(t => t.TAR_FEC_CRE.Year == anio);
                        break;
                    case "Mes":
                        var meses = System.Globalization.CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.MonthNames;
                        int numMes = Array.FindIndex(meses, m => m.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase)) + 1;
                        if (numMes > 0) resultado = resultado.Where(t => t.TAR_FEC_CRE.Month == numMes);
                        break;
                    case "Dia":
                        if (int.TryParse(filtro.Etiqueta, out int dia))
                            resultado = resultado.Where(t => t.TAR_FEC_CRE.Day == dia);
                        break;
                    case "Tarea":
                        resultado = resultado.Where(t => t.TAR_ID == filtro.Id);
                        break;
                    case "Proceso":
                        resultado = resultado.Where(t => t.PROC_ID == filtro.Id);
                        break;
                    case "Proyecto":
                        var procIds = ProcesosList.Where(p => p.PRO_ID == filtro.Id).Select(p => p.PROC_ID).ToList();
                        resultado = resultado.Where(t => t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value));
                        break;
                    case "Area":
                        var proyIds = ProyectosList.Where(p => p.ARE_ID == filtro.Id).Select(p => p.PRO_ID).ToList();
                        var prIds = ProcesosList.Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToList();
                        resultado = resultado.Where(t => t.PROC_ID.HasValue && prIds.Contains(t.PROC_ID.Value));
                        break;
                    case "Prioridad":
                        resultado = resultado.Where(t => t.PRI_ID == filtro.Id);
                        break;
                }
            }

            TareasFiltradas = resultado.OrderByDescending(t => t.TAR_FEC_CRE).ToList();
            StateHasChanged();
        }

        // ── DETALLE ─────────────────────────────────────────────
        private void AbrirDetalleTarea(TareaModel tarea)
        {
            TareaSeleccionada = tarea;
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            TareaSeleccionada = null;
            StateHasChanged();
        }

        // ── VER DETALLE COMPLETO (DetalleTarea) ─────────────────
        private void AbrirDetalleTareaCompleta()
        {
            if (TareaSeleccionada == null) return;
            TareaParaDetalle = TareaSeleccionada;
            TareaSeleccionada = null;
            StateHasChanged();
        }

        private void CerrarDetalleTareaCompleta()
        {
            TareaParaDetalle = null;
            StateHasChanged();
        }

        // ── HELPERS ─────────────────────────────────────────────
        private string ObtenerNombreProceso(Guid? id)
        {
            if (!id.HasValue) return "Sin proceso";
            return ProcesosGlobales.FirstOrDefault(p => p.PROC_ID == id.Value)?.PROC_NOM ?? "Proceso no encontrado";
        }

        private string ObtenerNombreProyecto(Guid? procesoId)
        {
            if (!procesoId.HasValue) return "Sin proyecto";
            var proc = ProcesosGlobales.FirstOrDefault(p => p.PROC_ID == procesoId.Value);
            if (proc == null || !proc.PRO_ID.HasValue) return "Proyecto no encontrado";
            return ProyectosGlobales.FirstOrDefault(p => p.PRO_ID == proc.PRO_ID.Value)?.PRO_NOM ?? "Proyecto no encontrado";
        }

        private string ObtenerNombrePrioridad(Guid? id)
        {
            if (!id.HasValue) return "Sin prioridad";
            return PrioridadesList.FirstOrDefault(p => p.PRI_ID == id.Value)?.PRI_NOM ?? "Baja";
        }

        private bool EsGerente(string rol)
        {
            var roles = new[] { "Gerente", "Administrador", "Enoc", "Admin" };
            return roles.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        private bool EsLiderTecnico(string rol)
        {
            var roles = new[] { "Lider Tecnico", "LiderTecnico", "Lider" };
            return roles.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        private async Task Regresar() => await OnClose.InvokeAsync();
    }
}
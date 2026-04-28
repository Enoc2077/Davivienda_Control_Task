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
    public partial class BitacoraSolucion : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }

        private List<SolucionesModel> SolucionesGlobales = new();
        private List<SolucionesModel> SolucionesFiltradas = new();
        private List<FriccionModel> FriccionesGlobales = new();

        private List<AreasModel> AreasList = new();
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosList = new();
        private List<TareaModel> TareasList = new();
        private List<PrioridadModel> PrioridadesList = new();

        public SolucionesModel? SolucionSeleccionada { get; set; }
        private FriccionModel? FriccionAsociada { get; set; }

        // Tarea que se abre en DetalleTarea al pulsar "Ver Tarea Padre"
        private TareaModel? TareaParaDetalle { get; set; }

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
                var resSol = await Client.GetSoluciones.ExecuteAsync();
                SolucionesGlobales = resSol.Data?.Soluciones?.Select(s => new SolucionesModel
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

                var resFri = await Client.GetFricciones.ExecuteAsync();
                FriccionesGlobales = resFri.Data?.Fricciones?.Select(f => new FriccionModel
                {
                    FRI_ID = f.Fri_ID,
                    TAR_ID = f.Tar_ID,
                    FRI_TIP = f.Fri_TIP,
                    FRI_DES = f.Fri_DES,
                    FRI_EST = f.Fri_EST,
                    FRI_IMP = f.Fri_IMP
                }).ToList() ?? new();

                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasList = resTar.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    TAR_DES = t.Tar_DES,
                    TAR_EST = t.Tar_EST,
                    PROC_ID = t.Proc_ID,
                    PRI_ID = t.Pri_ID,
                    USU_ID = t.Usu_ID,
                    TAR_FEC_INI = t.Tar_FEC_INI.DateTime,
                    TAR_FEC_FIN = t.Tar_FEC_FIN?.DateTime,
                    TAR_FEC_CRE = t.Tar_FEC_CRE,
                    TAR_FEC_MOD = t.Tar_FEC_MOD
                }).ToList() ?? new();

                var resProc = await Client.GetProcesos.ExecuteAsync();
                ProcesosList = resProc.Data?.Procesos.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PRO_ID = p.Pro_ID
                }).ToList() ?? new();

                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosList = resProy.Data?.Proyectos.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    ARE_ID = p.Are_ID
                }).ToList() ?? new();

                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();

                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                PrioridadesList = resPrio.Data?.Prioridades.Select(p => new PrioridadModel
                {
                    PRI_ID = p.Pri_ID,
                    PRI_NOM = p.Pri_NOM
                }).ToList() ?? new();

                SolucionesGlobales = FiltrarSolucionesPorRol(SolucionesGlobales);
                FiltrarListasPorRol();

                SolucionesFiltradas = SolucionesGlobales
                    .OrderByDescending(s => s.SOL_FEC_CRE)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CargarTodo: {ex.Message}");
            }
        }

        // ── FILTRADO POR ROL ────────────────────────────────────
        private List<SolucionesModel> FiltrarSolucionesPorRol(List<SolucionesModel> todas)
        {
            if (EsGerente(UserRole)) return todas;
            if (!UserAreaId.HasValue) return new();

            var proyDelArea = ProyectosList.Where(p => p.ARE_ID == UserAreaId).Select(p => p.PRO_ID).ToHashSet();
            var procDelArea = ProcesosList.Where(p => p.PRO_ID.HasValue && proyDelArea.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToHashSet();
            var tareasDelArea = TareasList.Where(t => t.PROC_ID.HasValue && procDelArea.Contains(t.PROC_ID.Value)).Select(t => t.TAR_ID).ToHashSet();
            var fricDelArea = FriccionesGlobales.Where(f => f.TAR_ID.HasValue && tareasDelArea.Contains(f.TAR_ID.Value)).Select(f => f.FRI_ID).ToHashSet();

            return todas.Where(s => s.FRI_ID.HasValue && fricDelArea.Contains(s.FRI_ID.Value)).ToList();
        }

        private void FiltrarListasPorRol()
        {
            if (EsGerente(UserRole)) return;
            if (!UserAreaId.HasValue) { AreasList = new(); ProyectosList = new(); ProcesosList = new(); TareasList = new(); return; }

            AreasList = AreasList.Where(a => a.ARE_ID == UserAreaId).ToList();
            var proyIds = ProyectosList.Where(p => p.ARE_ID == UserAreaId).Select(p => p.PRO_ID).ToHashSet();
            ProyectosList = ProyectosList.Where(p => proyIds.Contains(p.PRO_ID)).ToList();
            var procIds = ProcesosList.Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToHashSet();
            ProcesosList = ProcesosList.Where(p => procIds.Contains(p.PROC_ID)).ToList();
            TareasList = TareasList.Where(t => t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value)).ToList();
        }

        // ── FILTROS ─────────────────────────────────────────────
        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            if (filtros == null || !filtros.Any())
            {
                SolucionesFiltradas = SolucionesGlobales.OrderByDescending(s => s.SOL_FEC_CRE).ToList();
                StateHasChanged();
                return;
            }

            var resultado = SolucionesGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(s => s.SOL_NOM.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                    case "Año":
                        if (int.TryParse(filtro.Etiqueta, out int anio))
                            resultado = resultado.Where(s => s.SOL_FEC_CRE.Year == anio);
                        break;
                    case "Mes":
                        var meses = System.Globalization.CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.MonthNames;
                        int numMes = Array.FindIndex(meses, m => m.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase)) + 1;
                        if (numMes > 0) resultado = resultado.Where(s => s.SOL_FEC_CRE.Month == numMes);
                        break;
                    case "Dia":
                        if (int.TryParse(filtro.Etiqueta, out int dia))
                            resultado = resultado.Where(s => s.SOL_FEC_CRE.Day == dia);
                        break;
                    case "Tarea":
                        var fricIds = FriccionesGlobales.Where(f => f.TAR_ID == filtro.Id).Select(f => f.FRI_ID).ToList();
                        resultado = resultado.Where(s => s.FRI_ID.HasValue && fricIds.Contains(s.FRI_ID.Value));
                        break;
                    case "Proceso":
                        var tarIds = TareasList.Where(t => t.PROC_ID == filtro.Id).Select(t => t.TAR_ID).ToList();
                        var friIdsPro = FriccionesGlobales.Where(f => f.TAR_ID.HasValue && tarIds.Contains(f.TAR_ID.Value)).Select(f => f.FRI_ID).ToList();
                        resultado = resultado.Where(s => s.FRI_ID.HasValue && friIdsPro.Contains(s.FRI_ID.Value));
                        break;
                    case "Proyecto":
                        var procIds = ProcesosList.Where(p => p.PRO_ID == filtro.Id).Select(p => p.PROC_ID).ToList();
                        var tIds = TareasList.Where(t => t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value)).Select(t => t.TAR_ID).ToList();
                        var fIds = FriccionesGlobales.Where(f => f.TAR_ID.HasValue && tIds.Contains(f.TAR_ID.Value)).Select(f => f.FRI_ID).ToList();
                        resultado = resultado.Where(s => s.FRI_ID.HasValue && fIds.Contains(s.FRI_ID.Value));
                        break;
                    case "Area":
                        var proyIds = ProyectosList.Where(p => p.ARE_ID == filtro.Id).Select(p => p.PRO_ID).ToList();
                        var prIds = ProcesosList.Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToList();
                        var taIds = TareasList.Where(t => t.PROC_ID.HasValue && prIds.Contains(t.PROC_ID.Value)).Select(t => t.TAR_ID).ToList();
                        var frIds = FriccionesGlobales.Where(f => f.TAR_ID.HasValue && taIds.Contains(f.TAR_ID.Value)).Select(f => f.FRI_ID).ToList();
                        resultado = resultado.Where(s => s.FRI_ID.HasValue && frIds.Contains(s.FRI_ID.Value));
                        break;
                    case "Prioridad":
                        resultado = resultado.Where(s => s.SOL_EST.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }

            SolucionesFiltradas = resultado.OrderByDescending(s => s.SOL_FEC_CRE).ToList();
            StateHasChanged();
        }

        // ── DETALLE SOLUCION ────────────────────────────────────
        private void AbrirDetalleSolucion(SolucionesModel sol)
        {
            SolucionSeleccionada = sol;
            FriccionAsociada = sol.FRI_ID.HasValue
                ? FriccionesGlobales.FirstOrDefault(f => f.FRI_ID == sol.FRI_ID.Value)
                : null;
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            SolucionSeleccionada = null;
            FriccionAsociada = null;
            StateHasChanged();
        }

        // ── VER TAREA PADRE — mismo patron que AbrirDetalleTarea en Asignaciones ──
        private void VerTareaPadre()
        {
            if (SolucionSeleccionada == null) return;

            // 1. Buscar friccion de la solucion
            var fri = SolucionSeleccionada.FRI_ID.HasValue
                ? FriccionesGlobales.FirstOrDefault(f => f.FRI_ID == SolucionSeleccionada.FRI_ID.Value)
                : null;

            if (fri?.TAR_ID == null) return;

            // 2. Buscar la tarea completa — con todos los campos que necesita DetalleTarea
            var tarea = TareasList.FirstOrDefault(t => t.TAR_ID == fri.TAR_ID.Value);
            if (tarea == null) return;

            // 3. Cerrar el modal de solucion y abrir DetalleTarea
            SolucionSeleccionada = null;
            FriccionAsociada = null;
            TareaParaDetalle = tarea;
            StateHasChanged();
        }

        private void CerrarDetalleTarea()
        {
            TareaParaDetalle = null;
            StateHasChanged();
        }

        // ── HELPERS ─────────────────────────────────────────────
        private string ObtenerNombreTarea(Guid? friccionId)
        {
            if (!friccionId.HasValue) return "Sin tarea vinculada";
            var fri = FriccionesGlobales.FirstOrDefault(f => f.FRI_ID == friccionId.Value);
            if (fri == null || !fri.TAR_ID.HasValue) return "Tarea no encontrada";
            return TareasList.FirstOrDefault(t => t.TAR_ID == fri.TAR_ID.Value)?.TAR_NOM ?? "Nombre no disponible";
        }

        private string ObtenerClaseEstadoFriccion(string? estado) => estado switch
        {
            "Abierta" => "friccion-estado-abierta",
            "En Análisis" => "friccion-estado-analisis",
            "Mitigada" => "friccion-estado-mitigada",
            _ => "friccion-estado-abierta"
        };

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
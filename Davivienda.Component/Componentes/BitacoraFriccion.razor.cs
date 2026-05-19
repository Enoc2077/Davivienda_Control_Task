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
    public partial class BitacoraFriccion : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }

        private List<FriccionModel> FriccionesGlobales = new();
        private List<FriccionModel> FriccionesFiltradas = new();

        private List<AreasModel> AreasList = new();
        private List<ProyectosModel> ProyectosList = new();
        private List<ProcesoModel> ProcesosGlobales = new();
        private List<TareaModel> TareasGlobales = new();
        private List<PrioridadModel> PrioridadesList = new();

        private FriccionModel? FriccionSeleccionada;

        // Tarea que se abre en DetalleTarea
        private TareaModel? TareaParaDetalle;

        private Dictionary<Guid, string> ColoresTareas = new();
        private string[] PaletaColores = { "#ef4444", "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6" };

        // Rol y area
        private string UserRole = "";
        private Guid? UserAreaId;
        private UsuarioModel? UsuarioActual;

        private bool PuedeVerTarea => EsGerente(UserRole) || EsLiderTecnico(UserRole);

        protected override async Task OnInitializedAsync()
        {
            await CargarUsuarioYRol();
            await CargarDatos();
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

        // ── CARGA ────────────────────────────────────────────────
        private async Task CargarDatos()
        {
            try
            {
                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();

                var resPro = await Client.GetProyectos.ExecuteAsync();
                ProyectosList = resPro.Data?.Proyectos?.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    ARE_ID = p.Are_ID
                }).ToList() ?? new();

                var resPrc = await Client.GetProcesos.ExecuteAsync();
                ProcesosGlobales = resPrc.Data?.Procesos?.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PRO_ID = p.Pro_ID
                }).ToList() ?? new();

                var resTar = await Client.GetTareas.ExecuteAsync();
                TareasGlobales = resTar.Data?.Tareas?.Select(t => new TareaModel
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

                var resFri = await Client.GetFricciones.ExecuteAsync();
                FriccionesGlobales = resFri.Data?.Fricciones?.Select(f => new FriccionModel
                {
                    FRI_ID = f.Fri_ID,
                    FRI_TIP = f.Fri_TIP,
                    FRI_DES = f.Fri_DES,
                    FRI_EST = f.Fri_EST,
                    FRI_IMP = f.Fri_IMP,
                    TAR_ID = f.Tar_ID,
                    FRI_FEC_CRE = f.Fri_FEC_CRE.DateTime
                }).ToList() ?? new();

                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                PrioridadesList = resPrio.Data?.Prioridades.Select(p => new PrioridadModel
                {
                    PRI_ID = p.Pri_ID,
                    PRI_NOM = p.Pri_NOM
                }).ToList() ?? new();

                // Colores por tarea
                int i = 0;
                foreach (var t in TareasGlobales)
                    if (!ColoresTareas.ContainsKey(t.TAR_ID))
                        ColoresTareas[t.TAR_ID] = PaletaColores[i++ % PaletaColores.Length];

                // Filtrar por rol
                FriccionesGlobales = FiltrarFriccionesPorRol(FriccionesGlobales);
                FiltrarListasPorRol();

                FriccionesFiltradas = FriccionesGlobales;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CargarDatos: {ex.Message}");
            }
        }

        // ── FILTRADO POR ROL ────────────────────────────────────
        private List<FriccionModel> FiltrarFriccionesPorRol(List<FriccionModel> todas)
        {
            if (EsGerente(UserRole)) return todas;
            if (!UserAreaId.HasValue) return new();

            var proyDelArea = ProyectosList.Where(p => p.ARE_ID == UserAreaId).Select(p => p.PRO_ID).ToHashSet();
            var procDelArea = ProcesosGlobales.Where(p => p.PRO_ID.HasValue && proyDelArea.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToHashSet();
            var tareasDelArea = TareasGlobales.Where(t => t.PROC_ID.HasValue && procDelArea.Contains(t.PROC_ID.Value)).Select(t => t.TAR_ID).ToHashSet();

            return todas.Where(f => f.TAR_ID.HasValue && tareasDelArea.Contains(f.TAR_ID.Value)).ToList();
        }

        private void FiltrarListasPorRol()
        {
            if (EsGerente(UserRole)) return;
            if (!UserAreaId.HasValue) { AreasList = new(); ProyectosList = new(); ProcesosGlobales = new(); TareasGlobales = new(); return; }

            AreasList = AreasList.Where(a => a.ARE_ID == UserAreaId).ToList();
            var proyIds = ProyectosList.Where(p => p.ARE_ID == UserAreaId).Select(p => p.PRO_ID).ToHashSet();
            ProyectosList = ProyectosList.Where(p => proyIds.Contains(p.PRO_ID)).ToList();
            var procIds = ProcesosGlobales.Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToHashSet();
            ProcesosGlobales = ProcesosGlobales.Where(p => procIds.Contains(p.PROC_ID)).ToList();
            TareasGlobales = TareasGlobales.Where(t => t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value)).ToList();
        }

        // ── FILTROS ─────────────────────────────────────────────
        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            if (filtros == null || !filtros.Any())
            {
                FriccionesFiltradas = FriccionesGlobales;
                StateHasChanged();
                return;
            }

            var resultado = FriccionesGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(f => f.FRI_TIP.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                    case "Año":
                        if (int.TryParse(filtro.Etiqueta, out int anio))
                            resultado = resultado.Where(f => f.FRI_FEC_CRE.Year == anio);
                        break;
                    case "Mes":
                        var meses = System.Globalization.CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.MonthNames;
                        int numMes = Array.FindIndex(meses, m => m.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase)) + 1;
                        if (numMes > 0) resultado = resultado.Where(f => f.FRI_FEC_CRE.Month == numMes);
                        break;
                    case "Dia":
                        if (int.TryParse(filtro.Etiqueta, out int dia))
                            resultado = resultado.Where(f => f.FRI_FEC_CRE.Day == dia);
                        break;
                    case "Tarea":
                        resultado = resultado.Where(f => f.TAR_ID == filtro.Id);
                        break;
                    case "Proceso":
                        var tarIds = TareasGlobales.Where(t => t.PROC_ID == filtro.Id).Select(t => t.TAR_ID).ToList();
                        resultado = resultado.Where(f => f.TAR_ID.HasValue && tarIds.Contains(f.TAR_ID.Value));
                        break;
                    case "Proyecto":
                        var procIds = ProcesosGlobales.Where(p => p.PRO_ID == filtro.Id).Select(p => p.PROC_ID).ToList();
                        var tIds = TareasGlobales.Where(t => t.PROC_ID.HasValue && procIds.Contains(t.PROC_ID.Value)).Select(t => t.TAR_ID).ToList();
                        resultado = resultado.Where(f => f.TAR_ID.HasValue && tIds.Contains(f.TAR_ID.Value));
                        break;
                    case "Area":
                        var proyIds = ProyectosList.Where(p => p.ARE_ID == filtro.Id).Select(p => p.PRO_ID).ToList();
                        var prIds = ProcesosGlobales.Where(p => p.PRO_ID.HasValue && proyIds.Contains(p.PRO_ID.Value)).Select(p => p.PROC_ID).ToList();
                        var taIds = TareasGlobales.Where(t => t.PROC_ID.HasValue && prIds.Contains(t.PROC_ID.Value)).Select(t => t.TAR_ID).ToList();
                        resultado = resultado.Where(f => f.TAR_ID.HasValue && taIds.Contains(f.TAR_ID.Value));
                        break;
                    case "Prioridad":
                        resultado = resultado.Where(f => f.FRI_IMP.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }

            FriccionesFiltradas = resultado.ToList();
            StateHasChanged();
        }

        // ── DETALLE ─────────────────────────────────────────────
        private void AbrirDetalleFriccion(FriccionModel fri)
        {
            FriccionSeleccionada = fri;
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            FriccionSeleccionada = null;
            StateHasChanged();
        }

        // ── VER TAREA PADRE ─────────────────────────────────────
        private void VerTareaPadre()
        {
            if (FriccionSeleccionada?.TAR_ID == null) return;

            var tarea = TareasGlobales.FirstOrDefault(t => t.TAR_ID == FriccionSeleccionada.TAR_ID.Value);
            if (tarea == null) return;

            FriccionSeleccionada = null;
            TareaParaDetalle = tarea;
            StateHasChanged();
        }

        private void CerrarDetalleTarea()
        {
            TareaParaDetalle = null;
            StateHasChanged();
        }

        // ── HELPERS ─────────────────────────────────────────────
        private string ObtenerColorTarea(Guid? id) =>
            (id.HasValue && ColoresTareas.ContainsKey(id.Value)) ? ColoresTareas[id.Value] : "#cbd5e1";

        private string ObtenerNombreTarea(Guid? id) =>
            TareasGlobales.FirstOrDefault(t => t.TAR_ID == id)?.TAR_NOM ?? "Sin Tarea";

        private string ObtenerClaseEstado(string? estado) => estado switch
        {
            "Abierta" => "estado-abierta",
            "En Análisis" => "estado-analisis",
            "Mitigada" => "estado-mitigada",
            _ => "estado-abierta"
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
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
    public partial class Bitacoraproyecto : ComponentBase
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Parameter] public EventCallback OnClose { get; set; }

        private List<ProyectosModel> ProyectosGlobales = new();
        private List<ProyectosModel> ProyectosFiltrados = new();
        private List<AreasModel> AreasList = new();
        private List<ProcesoModel> ProcesosGlobales = new();

        // Lista para el componente Filtros (solo proyectos, mapeados como ProyectosModel)
        private List<ProyectosModel> ProyectosParaFiltro = new();

        // Proyecto seleccionado para el detalle
        private ProyectosModel? ProyectoSeleccionado;

        // Control modal editar
        private bool MostrarEditar = false;

        // Rol y area
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
                // 1. Proyectos finalizados
                var resProy = await Client.GetProyectos.ExecuteAsync();
                var todos = resProy.Data?.Proyectos.Select(p => new ProyectosModel
                {
                    PRO_ID = p.Pro_ID,
                    PRO_NOM = p.Pro_NOM,
                    PRO_DES = p.Pro_DES,
                    PRO_EST = p.Pro_EST,
                    ARE_ID = p.Are_ID,
                    PRO_FEC_INI = p.Pro_FEC_INI,
                    PRO_FEC_FIN = p.Pro_FEC_FIN,
                    PRO_FEC_CRE = p.Pro_FEC_CRE,
                    PRO_FEC_MOD = p.Pro_FEC_MOD
                }).ToList() ?? new();

                ProyectosGlobales = todos
                    .Where(p => p.PRO_EST == "FINALIZADO")
                    .OrderByDescending(p => p.PRO_FEC_FIN ?? p.PRO_FEC_CRE)
                    .ToList();

                // Filtrar por rol y area
                ProyectosGlobales = FiltrarPorRol(ProyectosGlobales);

                // 2. Áreas
                var resArea = await Client.GetAreas.ExecuteAsync();
                AreasList = resArea.Data?.Areas.Select(a => new AreasModel
                {
                    ARE_ID = a.Are_ID,
                    ARE_NOM = a.Are_NOM
                }).ToList() ?? new();

                // Filtrar areas segun rol
                if (!EsGerente(UserRole) && UserAreaId.HasValue)
                    AreasList = AreasList.Where(a => a.ARE_ID == UserAreaId).ToList();

                // 3. Procesos
                var resProc = await Client.GetProcesos.ExecuteAsync();
                ProcesosGlobales = resProc.Data?.Procesos.Select(p => new ProcesoModel
                {
                    PROC_ID = p.Proc_ID,
                    PROC_NOM = p.Proc_NOM,
                    PROC_EST = p.Proc_EST,
                    PRO_ID = p.Pro_ID
                }).ToList() ?? new();

                // Lista para el componente Filtros
                ProyectosParaFiltro = ProyectosGlobales;

                ProyectosFiltrados = ProyectosGlobales.ToList();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CargarTodo: {ex.Message}");
            }
        }

        private List<ProyectosModel> FiltrarPorRol(List<ProyectosModel> todos)
        {
            if (EsGerente(UserRole)) return todos;
            if (!UserAreaId.HasValue) return new();
            return todos.Where(p => p.ARE_ID == UserAreaId).ToList();
        }

        // ── FILTROS via componente Filtros ───────────────────────
        private void ManejarCambioFiltros(List<FiltroActivoModel> filtros)
        {
            if (filtros == null || !filtros.Any())
            {
                ProyectosFiltrados = ProyectosGlobales.ToList();
                StateHasChanged();
                return;
            }

            var resultado = ProyectosGlobales.AsEnumerable();

            foreach (var filtro in filtros)
            {
                switch (filtro.Tipo)
                {
                    case "Nombre":
                        resultado = resultado.Where(p =>
                            p.PRO_NOM.Contains(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase));
                        break;
                    case "Proyecto":
                        resultado = resultado.Where(p => p.PRO_ID == filtro.Id);
                        break;
                    case "Area":
                        resultado = resultado.Where(p => p.ARE_ID == filtro.Id);
                        break;
                    case "Año":
                        if (int.TryParse(filtro.Etiqueta, out int anio))
                            resultado = resultado.Where(p => p.PRO_FEC_CRE.Year == anio);
                        break;
                    case "Mes":
                        var meses = System.Globalization.CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.MonthNames;
                        int numMes = Array.FindIndex(meses, m => m.Equals(filtro.Etiqueta, StringComparison.OrdinalIgnoreCase)) + 1;
                        if (numMes > 0) resultado = resultado.Where(p => p.PRO_FEC_CRE.Month == numMes);
                        break;
                    case "Dia":
                        if (int.TryParse(filtro.Etiqueta, out int dia))
                            resultado = resultado.Where(p => p.PRO_FEC_CRE.Day == dia);
                        break;
                }
            }

            ProyectosFiltrados = resultado
                .OrderByDescending(p => p.PRO_FEC_FIN ?? p.PRO_FEC_CRE)
                .ToList();
            StateHasChanged();
        }

        // ── DETALLE ─────────────────────────────────────────────
        private void AbrirDetalleProyecto(ProyectosModel proyecto)
        {
            ProyectoSeleccionado = proyecto;
            MostrarEditar = false;
            StateHasChanged();
        }

        private void CerrarDetalle()
        {
            ProyectoSeleccionado = null;
            MostrarEditar = false;
            StateHasChanged();
        }

        // ── EDITAR ──────────────────────────────────────────────
        private void AbrirEditar()
        {
            MostrarEditar = true;
            StateHasChanged();
        }

        private async Task CerrarEditar()
        {
            MostrarEditar = false;
            ProyectoSeleccionado = null;
            // Recargar para reflejar si el proyecto cambió de estado
            await CargarTodo();
            StateHasChanged();
        }

        // ── HELPERS ─────────────────────────────────────────────
        private string ObtenerNombreArea(Guid? areaId)
        {
            if (!areaId.HasValue) return "Sin área";
            return AreasList.FirstOrDefault(a => a.ARE_ID == areaId.Value)?.ARE_NOM ?? "Área no encontrada";
        }

        private int ObtenerCantidadProcesos(Guid proyectoId) =>
            ProcesosGlobales.Count(p => p.PRO_ID == proyectoId);

        private bool EsGerente(string rol)
        {
            var roles = new[] { "Gerente", "Administrador", "Enoc", "Admin" };
            return roles.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        private async Task Regresar() => await OnClose.InvokeAsync();
    }
}
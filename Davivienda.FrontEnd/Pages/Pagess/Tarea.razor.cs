using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Linq;
using System.Security.Claims;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Tarea
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        private List<TareaModel> TareasLista { get; set; } = new();
        private List<ProyectosModel> ProyectosLista { get; set; } = new();
        private List<ProcesoModel> TodosLosProcesos { get; set; } = new();
        private List<ProcesoModel> ProcesosFiltrados { get; set; } = new();
        private List<AreasModel> AreasLista { get; set; } = new();
        private List<UsuarioModel> UsuariosGlobales { get; set; } = new();
        private List<UsuarioModel> UsuariosFiltrados { get; set; } = new();
        private List<PrioridadModel> PrioridadesLista { get; set; } = new();

        private string UserRole { get; set; } = "";
        private Guid? UserAreaId { get; set; }
        private bool EsGerente { get; set; } = false;

        private string NuevaTareaNombre = "";
        private string NuevaTareaDesc = "";
        private string SelectedProcesoId = "";
        private Guid? NuevaTareaUsuarioId;
        private Guid? NuevaTareaPrioridadId;
        private DateTime NuevaTareaFechaIni = DateTime.Today;
        private DateTime NuevaTareaFechaFin = DateTime.Today.AddDays(3);

        private bool IsProcessing = false;
        private bool MostrarModal = false;
        private bool MostrarBitacora = false;
        private TareaModel TareaEdicion = new();

        protected override async Task OnInitializedAsync()
        {
            await ObtenerDatosUsuario();
            await CargarCatalogos();
            await CargarTareas();
        }

        private async Task ObtenerDatosUsuario()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                UserRole = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
                EsGerente = UserRole.Equals("Gerente", StringComparison.OrdinalIgnoreCase) ||
                            UserRole.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
                            UserRole.Equals("Enoc", StringComparison.OrdinalIgnoreCase);

                var usuNumClaim = user.FindFirst("USU_NUM")?.Value ?? "";
                if (!string.IsNullOrEmpty(usuNumClaim))
                {
                    var resUsuarios = await Client.GetUsuarios.ExecuteAsync();
                    var usuData = resUsuarios.Data?.Usuarios.FirstOrDefault(u => u.Usu_NUM == usuNumClaim);
                    if (usuData != null) UserAreaId = usuData.Are_ID;
                }
            }
        }

        private async Task CargarCatalogos()
        {
            var resAreas = await Client.GetAreas.ExecuteAsync();
            AreasLista = resAreas.Data?.Areas.Select(a => new AreasModel
            {
                ARE_ID = a.Are_ID,
                ARE_NOM = a.Are_NOM
            }).ToList() ?? new();

            var resProy = await Client.GetProyectos.ExecuteAsync();
            var proyectosData = resProy.Data?.Proyectos.Select(p => new ProyectosModel
            {
                PRO_ID = p.Pro_ID,
                PRO_NOM = p.Pro_NOM,
                ARE_ID = p.Are_ID
            }).ToList() ?? new();

            ProyectosLista = EsGerente ? proyectosData : proyectosData.Where(p => p.ARE_ID == UserAreaId).ToList();

            var resProc = await Client.GetProcesos.ExecuteAsync();
            TodosLosProcesos = resProc.Data?.Procesos.Select(p => new ProcesoModel
            {
                PROC_ID = p.Proc_ID,
                PROC_NOM = p.Proc_NOM,
                PRO_ID = p.Pro_ID
            }).ToList() ?? new();

            var resPrio = await Client.GetPrioridades.ExecuteAsync();
            PrioridadesLista = resPrio.Data?.Prioridades.Select(p => new PrioridadModel
            {
                PRI_ID = p.Pri_ID,
                PRI_NOM = p.Pri_NOM
            }).ToList() ?? new();
            NuevaTareaPrioridadId = PrioridadesLista.FirstOrDefault()?.PRI_ID;

            var resUsu = await Client.GetUsuarios.ExecuteAsync();
            UsuariosGlobales = resUsu.Data?.Usuarios.Select(u => new UsuarioModel
            {
                USU_ID = u.Usu_ID,
                USU_NOM = u.Usu_NOM,
                ARE_ID = u.Are_ID
            }).ToList() ?? new();

            UsuariosFiltrados = EsGerente ? UsuariosGlobales : UsuariosGlobales.Where(u => u.ARE_ID == UserAreaId).ToList();
        }

        private async Task CargarTareas()
        {
            var res = await Client.GetTareas.ExecuteAsync();
            var todasTareas = res.Data?.Tareas.Select(t => new TareaModel
            {
                TAR_ID = t.Tar_ID,
                TAR_NOM = t.Tar_NOM,
                TAR_DES = t.Tar_DES,
                TAR_EST = t.Tar_EST,
                PRI_ID = t.Pri_ID,
                USU_ID = t.Usu_ID,
                PROC_ID = t.Proc_ID,
                TAR_FEC_INI = t.Tar_FEC_INI,
                TAR_FEC_FIN = t.Tar_FEC_FIN,
                TAR_FEC_CRE = t.Tar_FEC_CRE
            }).ToList() ?? new();

            if (EsGerente)
            {
                TareasLista = todasTareas;
            }
            else
            {
                var idsMisProyectos = ProyectosLista
                    .Select(p => p.PRO_ID)
                    .Where(id => id != Guid.Empty)
                    .ToList();

                var idsMisProcesos = TodosLosProcesos
                    .Where(pc => pc.PRO_ID.HasValue && idsMisProyectos.Contains(pc.PRO_ID.Value))
                    .Select(pc => pc.PROC_ID)
                    .ToList();

                TareasLista = todasTareas.Where(t => idsMisProcesos.Contains((Guid)t.PROC_ID)).ToList();
            }
        }

        private void OnAreaChanged(ChangeEventArgs e)
        {
            var areaId = e.Value?.ToString();
            if (string.IsNullOrEmpty(areaId))
                UsuariosFiltrados = EsGerente ? UsuariosGlobales : UsuariosGlobales.Where(u => u.ARE_ID == UserAreaId).ToList();
            else
                UsuariosFiltrados = UsuariosGlobales.Where(u => u.ARE_ID.ToString() == areaId).ToList();
        }

        private void OnProyectoChanged(ChangeEventArgs e)
        {
            var proyId = e.Value?.ToString();
            ProcesosFiltrados = TodosLosProcesos.Where(p => p.PRO_ID.ToString() == proyId).ToList();
        }

        private async Task GuardarNuevaTarea()
        {
            if (string.IsNullOrEmpty(SelectedProcesoId) || string.IsNullOrEmpty(NuevaTareaNombre)) return;
            IsProcessing = true;
            try
            {
                var input = new TareaModelInput
                {
                    Tar_ID = Guid.NewGuid(),
                    Tar_NOM = NuevaTareaNombre,
                    Tar_DES = NuevaTareaDesc,
                    Tar_EST = "Pendiente",
                    Proc_ID = Guid.Parse(SelectedProcesoId),
                    Usu_ID = NuevaTareaUsuarioId,
                    Pri_ID = NuevaTareaPrioridadId,
                    Tar_FEC_INI = NuevaTareaFechaIni,
                    Tar_FEC_FIN = NuevaTareaFechaFin,
                    Tar_FEC_CRE = DateTimeOffset.Now
                };
                await Client.InsertTarea.ExecuteAsync(input);
                NuevaTareaNombre = "";
                NuevaTareaDesc = "";
                await CargarTareas();
            }
            finally { IsProcessing = false; }
        }

        private void AbrirModalEdicion(TareaModel t)
        {
            TareaEdicion = t;
            MostrarModal = true;
        }

        private async Task GuardarEdicion()
        {
            try
            {
                var input = new TareaModelInput
                {
                    Tar_ID = TareaEdicion.TAR_ID,
                    Tar_NOM = TareaEdicion.TAR_NOM,
                    Tar_DES = TareaEdicion.TAR_DES,
                    Tar_EST = TareaEdicion.TAR_EST,
                    Usu_ID = TareaEdicion.USU_ID,
                    Pri_ID = TareaEdicion.PRI_ID,
                    Proc_ID = TareaEdicion.PROC_ID,
                    Tar_FEC_INI = TareaEdicion.TAR_FEC_INI,
                    Tar_FEC_FIN = TareaEdicion.TAR_FEC_FIN,
                    Tar_FEC_CRE = TareaEdicion.TAR_FEC_CRE == default ? DateTimeOffset.Now : TareaEdicion.TAR_FEC_CRE,
                    Tar_FEC_MOD = DateTimeOffset.Now
                };
                var res = await Client.UpdateTarea.ExecuteAsync(input);
                if (res.Errors.Count == 0)
                {
                    MostrarModal = false;
                    await CargarTareas();
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        private string GetPrioridadNombre(Guid? id) =>
            PrioridadesLista.FirstOrDefault(p => p.PRI_ID == id)?.PRI_NOM ?? "Baja";

        private async Task EliminarTarea(Guid id)
        {
            if (await JS.InvokeAsync<bool>("confirm", "¿Deseas eliminar esta tarea permanentemente?"))
            {
                await Client.DeleteTarea.ExecuteAsync(id);
                await CargarTareas();
            }
        }
    }
}
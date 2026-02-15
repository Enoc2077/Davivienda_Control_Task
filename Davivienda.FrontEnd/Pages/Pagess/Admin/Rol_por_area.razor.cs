using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public partial class Rol_por_area
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;

        private List<UsuarioModel> ListaUsuarios = new();
        private List<UsuarioModel> UsuariosFiltrados = new();
        private List<RolesModel> ListaRoles = new();
        private List<AreasModel> ListaAreas = new();

        private UsuarioModel? UsuarioSeleccionado;
        private Guid? RolIdSeleccionado;
        private Guid? AreaIdSeleccionada;
        private Guid? AreaFiltroId;
        private string Busqueda = "";
        private bool MostrarModal = false;

        protected override async Task OnInitializedAsync() => await CargarDatos();

        private async Task CargarDatos()
        {
            try
            {
                var resUsu = await Client.GetUsuarios.ExecuteAsync();
                ListaUsuarios = resUsu.Data?.Usuarios.Select(u => new UsuarioModel
                {
                    USU_ID = u.Usu_ID,
                    USU_NOM = u.Usu_NOM,
                    USU_NUM = u.Usu_NUM,
                    ROL_ID = u.Rol_ID,
                    ARE_ID = u.Are_ID,
                    USU_FEC_CRE = u.Usu_FEC_CRE // Vital para que no de error al actualizar
                }).ToList() ?? new();

                var resRol = await Client.GetRoles.ExecuteAsync();
                ListaRoles = resRol.Data?.Roles.Select(r => new RolesModel { ROL_ID = r.Rol_ID, ROL_NOM = r.Rol_NOM }).ToList() ?? new();

                var resArea = await Client.GetAreas.ExecuteAsync();
                ListaAreas = resArea.Data?.Areas.Select(a => new AreasModel { ARE_ID = a.Are_ID, ARE_NOM = a.Are_NOM }).ToList() ?? new();

                FiltrarUsuarios();
            }
            catch (Exception ex) { Console.WriteLine($"Error al cargar: {ex.Message}"); }
        }

        private void FiltrarUsuarios()
        {
            var data = ListaUsuarios.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(Busqueda))
                data = data.Where(u => u.USU_NOM.Contains(Busqueda, StringComparison.OrdinalIgnoreCase));

            if (AreaFiltroId.HasValue)
                data = data.Where(u => u.ARE_ID == AreaFiltroId);

            UsuariosFiltrados = data.ToList();
        }

        private string ObtenerNombreRol(Guid? id) => ListaRoles.FirstOrDefault(r => r.ROL_ID == id)?.ROL_NOM ?? "Sin Rol";
        private string ObtenerNombreArea(Guid? id) => ListaAreas.FirstOrDefault(a => a.ARE_ID == id)?.ARE_NOM ?? "Área no asignada";

        private void AbrirModalAsignacion(UsuarioModel usuario)
        {
            UsuarioSeleccionado = usuario;
            RolIdSeleccionado = usuario.ROL_ID;
            AreaIdSeleccionada = usuario.ARE_ID;
            MostrarModal = true;
        }

        private async Task GuardarAsignacion()
        {
            if (UsuarioSeleccionado == null) return;
            try
            {
                var input = new UsuarioModelInput
                {
                    Usu_ID = UsuarioSeleccionado.USU_ID,
                    Usu_NOM = UsuarioSeleccionado.USU_NOM,
                    Rol_ID = RolIdSeleccionado,
                    Are_ID = AreaIdSeleccionada,
                    Usu_FEC_CRE = UsuarioSeleccionado.USU_FEC_CRE,
                    Usu_FEC_MOD = DateTimeOffset.Now
                };

                var res = await Client.UpdateUsuario.ExecuteAsync(input);
                if (res.Data != null)
                {
                    MostrarModal = false;
                    await CargarDatos();
                    StateHasChanged();
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error en update: {ex.Message}"); }
        }

        private void CerrarModal() => MostrarModal = false;
        private void OnSearchChanged(ChangeEventArgs e) { Busqueda = e.Value?.ToString() ?? ""; FiltrarUsuarios(); }
        private void OnAreaFilterChanged(ChangeEventArgs e) { if (Guid.TryParse(e.Value?.ToString(), out Guid r)) AreaFiltroId = r; else AreaFiltroId = null; FiltrarUsuarios(); }
    }
}
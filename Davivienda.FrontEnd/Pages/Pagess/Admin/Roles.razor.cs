using Davivienda.GraphQL.SDK;
using Davivienda.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Davivienda.FrontEnd.Pages.Pagess.Admin
{
    public partial class Roles
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private List<RolesModel> ListaRoles = new();
        private List<RolesModel> RolesFiltrados = new();
        private RolesModel RolForm = new();
        private string Busqueda = "";
        private bool Editando = false;
        private bool MostrarModal = false;

        protected override async Task OnInitializedAsync() => await CargarDatos();

        private async Task CargarDatos()
        {
            try
            {
                var res = await Client.GetRoles.ExecuteAsync();
                ListaRoles = res.Data?.Roles.Select(r => new RolesModel
                {
                    ROL_ID = r.Rol_ID,
                    ROL_NOM = r.Rol_NOM,
                    ROL_DES = r.Rol_DES,
                    ROL_EST = r.Rol_EST,
                    ROL_FEC_CRE = r.Rol_FEC_CRE,
                    ROL_FEC_MOD = r.Rol_FEC_MOD
                }).ToList() ?? new();

                FiltrarRoles();
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }

        // Método para filtrado en tiempo real
        private void OnSearchChanged(ChangeEventArgs e)
        {
            Busqueda = e.Value?.ToString() ?? "";
            FiltrarRoles();
        }

        private void FiltrarRoles()
        {
            RolesFiltrados = string.IsNullOrWhiteSpace(Busqueda)
                ? ListaRoles
                : ListaRoles.Where(r => r.ROL_NOM.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private async Task GuardarCambios()
        {
            try
            {
                var input = new RolesModelInput
                {
                    Rol_ID = RolForm.ROL_ID,
                    Rol_NOM = RolForm.ROL_NOM,
                    Rol_DES = RolForm.ROL_DES,
                    Rol_EST = RolForm.ROL_EST,
                    Rol_FEC_CRE = (Editando ? RolForm.ROL_FEC_CRE : DateTimeOffset.Now) ?? DateTimeOffset.Now,
                    Rol_FEC_MOD = DateTimeOffset.Now
                };

                if (Editando) await Client.UpdateRol.ExecuteAsync(input);
                else await Client.InsertRol.ExecuteAsync(input);

                MostrarModal = false;
                await CargarDatos();
                StateHasChanged();
            }
            catch (Exception ex) { Console.WriteLine($"Error al guardar: {ex.Message}"); }
        }

        private void AbrirModalNuevo()
        {
            RolForm = new RolesModel { ROL_ID = Guid.NewGuid(), ROL_EST = true };
            Editando = false;
            MostrarModal = true;
        }

        private void AbrirModalEditar(RolesModel rol)
        {
            RolForm = new RolesModel
            {
                ROL_ID = rol.ROL_ID,
                ROL_NOM = rol.ROL_NOM,
                ROL_DES = rol.ROL_DES,
                ROL_EST = rol.ROL_EST,
                ROL_FEC_CRE = rol.ROL_FEC_CRE
            };
            Editando = true;
            MostrarModal = true;
        }

        private async Task EliminarRol(Guid id)
        {
            if (await JS.InvokeAsync<bool>("confirm", "¿Está seguro de eliminar este rol?"))
            {
                await Client.DeleteRol.ExecuteAsync(id);
                await CargarDatos();
            }
        }
    }
}
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

namespace Davivienda.Component.Component // 👈 Asegúrate que coincida con la carpeta
{
    public partial class NewProyect : ComponentBase
    {
        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        [Parameter] public ProyectosModel Proyecto { get; set; } = new();
        [Parameter] public EventCallback OnClose { get; set; }

        private List<AreasModel> ListaAreas = new();
        private string UserRole { get; set; } = "";
        private Guid? UserAreaId { get; set; }
        private bool EsUsuarioGerente { get; set; } = false;
        private bool CargandoDatos { get; set; } = true; // 👈 Para evitar errores de nulos en UI

        protected override async Task OnInitializedAsync()
        {
            CargandoDatos = true;
            try
            {
                await ObtenerDatosUsuario();
                await CargarAreas();

                if (Proyecto.PRO_FEC_INI == default) Proyecto.PRO_FEC_INI = DateTimeOffset.Now;
                if (Proyecto.PRO_FEC_FIN == default) Proyecto.PRO_FEC_FIN = DateTimeOffset.Now.AddMonths(1);
            }
            finally
            {
                CargandoDatos = false;
            }
        }

        private async Task ObtenerDatosUsuario()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                UserRole = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
                EsUsuarioGerente = EsGerente(UserRole);

                // Importante: USU_NUM debe estar en los Claims del Token
                var usuNumClaim = user.FindFirst("USU_NUM")?.Value ?? "";

                if (!string.IsNullOrEmpty(usuNumClaim))
                {
                    var resUsuarios = await Client.GetUsuarios.ExecuteAsync();
                    var usuData = resUsuarios.Data?.Usuarios.FirstOrDefault(u => u.Usu_NUM == usuNumClaim);

                    if (usuData != null)
                    {
                        UserAreaId = usuData.Are_ID;
                        if (!EsUsuarioGerente && UserAreaId.HasValue)
                        {
                            Proyecto.ARE_ID = UserAreaId.Value;
                        }
                    }
                }
            }
        }

        private async Task CargarAreas()
        {
            try
            {
                var res = await Client.GetAreas.ExecuteAsync();
                if (res.Data?.Areas != null)
                {
                    ListaAreas = res.Data.Areas.Select(a => new AreasModel
                    {
                        ARE_ID = a.Are_ID,
                        ARE_NOM = a.Are_NOM
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private bool EsGerente(string rol)
        {
            var rolesGerente = new[] { "Gerente", "Administrador", "Enoc", "Admin" };
            return rolesGerente.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        public async Task CerrarModal() => await OnClose.InvokeAsync();

        public async Task GuardarProyecto()
        {
            Console.WriteLine("💾 [DEBUG] Iniciando proceso de guardado...");
            Console.WriteLine($"   - Proyecto: {Proyecto.PRO_NOM}");
            Console.WriteLine($"   - Descripción: {Proyecto.PRO_DES}");
            Console.WriteLine($"   - Área ID: {Proyecto.ARE_ID}");
            Console.WriteLine($"   - Fecha Inicio: {Proyecto.PRO_FEC_INI}");
            Console.WriteLine($"   - Fecha Fin: {Proyecto.PRO_FEC_FIN}");

            if (!Proyecto.ARE_ID.HasValue || Proyecto.ARE_ID == Guid.Empty)
            {
                Console.WriteLine("❌ [ERROR] Intento de guardado fallido: ARE_ID es nulo o vacío.");
                return;
            }

            try
            {
                var input = new ProyectosModelInput
                {
                    Pro_ID = Proyecto.PRO_ID == Guid.Empty ? Guid.Empty : Proyecto.PRO_ID,
                    Pro_NOM = Proyecto.PRO_NOM,
                    Pro_DES = Proyecto.PRO_DES,
                    Pro_FEC_INI = Proyecto.PRO_FEC_INI,
                    Pro_FEC_FIN = Proyecto.PRO_FEC_FIN,
                    Pro_EST = Proyecto.PRO_EST,
                    Are_ID = Proyecto.ARE_ID.Value,
                    Pro_FEC_CRE = Proyecto.PRO_FEC_CRE,
                    Pro_FEC_MOD = DateTimeOffset.Now
                };

                if (Proyecto.PRO_ID == Guid.Empty)
                {
                    Console.WriteLine("➕ [INFO] Ejecutando inserción de nuevo proyecto vía GraphQL...");
                    var result = await Client.InsertProyecto.ExecuteAsync(input);
                    if (result.Errors.Any()) Console.WriteLine($"❌ [GQL ERROR] {result.Errors[0].Message}");
                    else Console.WriteLine("✅ [SUCCESS] Proyecto creado correctamente.");
                }
                else
                {
                    Console.WriteLine($"✏️ [INFO] Actualizando proyecto ID: {Proyecto.PRO_ID}...");
                    var result = await Client.UpdateProyecto.ExecuteAsync(input);
                    if (result.Errors.Any()) Console.WriteLine($"❌ [GQL ERROR] {result.Errors[0].Message}");
                    else Console.WriteLine("✅ [SUCCESS] Proyecto actualizado correctamente.");
                }

                await OnClose.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 [EXCEPTION] Error crítico en GuardarProyecto: {ex.Message}");
            }
        }
    }
}
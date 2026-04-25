using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Asignaciones
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;

        public List<TareaModel>? Tasks { get; set; } = new();
        public List<TareaModel> TareasFiltradas { get; set; } = new();
        public List<PrioridadModel>? Prioridades { get; set; }
        public List<ProyectosModel> ProyectosRelacionados { get; set; } = new();
        public UsuarioModel? UsuarioActual { get; set; }

        public string PrioridadSeleccionada { get; set; } = "Todas";
        public string UserRole { get; set; } = "";
        public Guid? UserAreaId { get; set; }
        public DateTime FechaCalendario { get; set; } = DateTime.Today;
        public List<CalendarDayAsignacion> DiasDelMesAsignaciones { get; set; } = new();
        public bool MostrarModal { get; set; }
        public string ModalActual { get; set; } = "";
        public TareaModel? TareaSeleccionada { get; set; }

        // 🔥 NUEVO: Para BitacoraTarea
        public bool MostrarBitacoraTarea { get; set; }

        public int TotalTareasCount { get; set; }
        public int TotalProyectos { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await CargarDatosDesdeBase();
            GenerarCalendarioMini();
        }

        private async Task CargarDatosDesdeBase()
        {
            try
            {
                var authState = await AuthProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                // 🔥 OBTENER ROL DEL USUARIO
                UserRole = user.FindFirst(ClaimTypes.Role)?.Value
                           ?? user.FindFirst("role")?.Value
                           ?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                           ?? "Empleado";

                Console.WriteLine("========================================");
                Console.WriteLine($"🔐 ROL DETECTADO: {UserRole}");

                // 🔥 1. CARGAR USUARIO ACTUAL POR NÚMERO DE EMPLEADO
                var usuNumClaim = user.FindFirst("USU_NUM")?.Value ?? "";

                Console.WriteLine($"🔢 Buscando usuario con USU_NUM: '{usuNumClaim}'");

                if (string.IsNullOrEmpty(usuNumClaim))
                {
                    Console.WriteLine("❌ No se encontró el claim USU_NUM en el token");
                    return;
                }

                // Obtener todos los usuarios y filtrar por USU_NUM
                var resUsuarios = await Client.GetUsuarios.ExecuteAsync();
                var usuData = resUsuarios.Data?.Usuarios.FirstOrDefault(u => u.Usu_NUM == usuNumClaim);

                if (usuData != null)
                {
                    UsuarioActual = new UsuarioModel
                    {
                        USU_ID = usuData.Usu_ID,
                        USU_NOM = usuData.Usu_NOM,
                        USU_NUM = usuData.Usu_NUM,
                        USU_COR = usuData.Usu_COR,
                        ARE_ID = usuData.Are_ID,
                        ROL_ID = usuData.Rol_ID
                    };

                    UserAreaId = UsuarioActual.ARE_ID;

                    Console.WriteLine($"👤 Usuario: {UsuarioActual.USU_NOM}");
                    Console.WriteLine($"🔢 Número: {UsuarioActual.USU_NUM}");
                    Console.WriteLine($"🏢 Área ID: {UserAreaId}");
                }
                else
                {
                    Console.WriteLine($"❌ Usuario con USU_NUM '{usuNumClaim}' no encontrado en la base de datos");
                    return;
                }

                // 🔥 2. CARGAR TODAS LAS TAREAS
                var response = await Client.GetTareas.ExecuteAsync();
                var allTasks = response.Data?.Tareas.Select(t => new TareaModel
                {
                    TAR_ID = t.Tar_ID,
                    TAR_NOM = t.Tar_NOM,
                    TAR_DES = t.Tar_DES,
                    TAR_EST = t.Tar_EST,
                    PRI_ID = t.Pri_ID,
                    USU_ID = t.Usu_ID,
                    TAR_FEC_INI = t.Tar_FEC_INI.DateTime,
                    TAR_FEC_FIN = t.Tar_FEC_FIN?.DateTime,
                    PROC_ID = t.Proc_ID
                }).ToList() ?? new();

                Console.WriteLine($"📊 Total de tareas en BD: {allTasks.Count}");

                // 🔥 2.1 CARGAR PROCESOS PARA VERIFICAR PROC_EST
                var resProc = await Client.GetProcesos.ExecuteAsync();
                var procesosActivos = resProc.Data?.Procesos
                    .Where(p => p.Proc_EST == true) // Solo procesos activos
                    .Select(p => p.Proc_ID)
                    .ToList() ?? new();

                Console.WriteLine($"📊 Procesos activos: {procesosActivos.Count}");

                // 🔥 2.2 FILTRAR TAREAS CON PROCESOS ACTIVOS
                allTasks = allTasks.Where(t =>
                {
                    // Si no tiene proceso, la mostramos (por seguridad)
                    if (!t.PROC_ID.HasValue) return true;

                    // Solo mostrar si el proceso está activo
                    return procesosActivos.Contains(t.PROC_ID.Value);
                }).ToList();

                Console.WriteLine($"📊 Tareas con proceso activo: {allTasks.Count}");

                // 🔥 3. FILTRAR TAREAS SEGÚN ROL Y ÁREA
                Tasks = await FiltrarTareasPorRolYArea(allTasks);

                Console.WriteLine($"✅ Tareas visibles para {UserRole}: {Tasks.Count}");
                Console.WriteLine("========================================");

                TareasFiltradas = Tasks;
                TotalTareasCount = Tasks.Count;

                // 4. CARGAR PROYECTOS
                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosRelacionados = resProy.Data?.Proyectos
                    .Select(p => new ProyectosModel
                    {
                        PRO_ID = p.Pro_ID,
                        PRO_NOM = p.Pro_NOM,
                        ARE_ID = p.Are_ID
                    })
                    .ToList() ?? new();
                TotalProyectos = ProyectosRelacionados.Count;

                // 5. CARGAR PRIORIDADES
                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                Prioridades = resPrio.Data?.Prioridades
                    .Select(p => new PrioridadModel { PRI_ID = p.Pri_ID, PRI_NOM = p.Pri_NOM })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine($"❌ ERROR EN DASHBOARD: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine("========================================");
            }
        }

        // 🔥 MÉTODO PRINCIPAL: FILTRAR TAREAS POR ROL Y ÁREA
        private async Task<List<TareaModel>> FiltrarTareasPorRolYArea(List<TareaModel> todasLasTareas)
        {
            // 🔥 ROL 1: GERENTE - Ve TODAS las tareas
            if (EsGerente(UserRole))
            {
                Console.WriteLine("👔 GERENTE/ADMIN: Acceso total a todas las tareas");
                return todasLasTareas;
            }

            // 🔥 ROL 2: LÍDER TÉCNICO - Ve solo tareas de SU área
            if (EsLiderTecnico(UserRole))
            {
                Console.WriteLine($"👨‍💼 LÍDER TÉCNICO: Filtrando tareas del área {UserAreaId}");

                if (!UserAreaId.HasValue)
                {
                    Console.WriteLine("⚠️ Líder sin área asignada, no verá tareas");
                    return new List<TareaModel>();
                }

                var tareasFiltradas = new List<TareaModel>();

                foreach (var tarea in todasLasTareas)
                {
                    if (tarea.PROC_ID.HasValue)
                    {
                        try
                        {
                            // 1. Obtener el PROCESO
                            var resProceso = await Client.GetProcesoById.ExecuteAsync(tarea.PROC_ID.Value);

                            if (resProceso.Data?.ProcesoById?.Pro_ID != null)
                            {
                                var proyectoId = resProceso.Data.ProcesoById.Pro_ID.Value;

                                // 2. Obtener el PROYECTO del proceso
                                var resProyecto = await Client.GetProyectoById.ExecuteAsync(proyectoId);

                                if (resProyecto.Data?.ProyectoById != null)
                                {
                                    var areaDelProyecto = resProyecto.Data.ProyectoById.Are_ID;

                                    // 3. Verificar si el ÁREA coincide
                                    if (areaDelProyecto == UserAreaId)
                                    {
                                        tareasFiltradas.Add(tarea);
                                        Console.WriteLine($"  ✓ Tarea '{tarea.TAR_NOM}' incluida");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error procesando tarea '{tarea.TAR_NOM}': {ex.Message}");
                        }
                    }
                }

                Console.WriteLine($"  → Total tareas del área: {tareasFiltradas.Count}");
                return tareasFiltradas;
            }

            // 🔥 ROL 3: EMPLEADO - Ve solo SUS tareas asignadas
            Console.WriteLine($"👤 EMPLEADO: Filtrando solo tareas asignadas");

            var tareasEmpleado = todasLasTareas
                .Where(t => t.USU_ID == UsuarioActual?.USU_ID)
                .ToList();

            Console.WriteLine($"  → {tareasEmpleado.Count} tareas asignadas");

            return tareasEmpleado;
        }

        // 🔥 HELPERS: Verificar roles
        private bool EsGerente(string rol)
        {
            var rolesGerente = new[] { "Gerente", "Administrador", "Enoc", "Admin" };
            return rolesGerente.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        private bool EsLiderTecnico(string rol)
        {
            var rolesLider = new[] { "Líder Técnico", "LiderTecnico", "Lider", "Líder" };
            return rolesLider.Any(r => rol.Equals(r, StringComparison.OrdinalIgnoreCase));
        }

        // MÉTODOS DE FILTRADO
        private void FiltrarPorPrioridad(string prio)
        {
            PrioridadSeleccionada = prio;
            TareasFiltradas = prio == "Todas"
                ? Tasks!
                : Tasks!.Where(t => GetPrioridadNombre(t.PRI_ID) == prio).ToList();
        }

        private void FiltrarPorEstadoGrafica(string estado)
        {
            if (estado == "Todas")
                TareasFiltradas = Tasks!;
            else
                TareasFiltradas = Tasks!.Where(t => t.TAR_EST == estado).ToList();
        }

        public string GetPrioridadNombre(Guid? id) =>
            Prioridades?.FirstOrDefault(p => p.PRI_ID == id)?.PRI_NOM ?? "Baja";

        // CALENDARIO
        public void GenerarCalendarioMini()
        {
            DiasDelMesAsignaciones.Clear();
            var primeroMes = new DateTime(FechaCalendario.Year, FechaCalendario.Month, 1);
            int offset = (int)primeroMes.DayOfWeek;
            var fechaInicio = primeroMes.AddDays(-offset);

            for (int i = 0; i < 35; i++)
            {
                var f = fechaInicio.AddDays(i);

                // 🔥 Verificar si hay tareas que INICIAN este día (verde)
                var tieneInicio = Tasks?.Any(t =>
                    t.TAR_FEC_INI.Date == f.Date &&
                    t.TAR_EST == "Pendiente") ?? false;

                // 🔥 Verificar si hay tareas que FINALIZAN este día (rojo)
                var tieneFin = Tasks?.Any(t =>
                    t.TAR_FEC_FIN.HasValue &&
                    t.TAR_FEC_FIN.Value.Date == f.Date &&
                    t.TAR_EST == "Pendiente") ?? false;

                DiasDelMesAsignaciones.Add(new CalendarDayAsignacion
                {
                    Fecha = f,
                    EsMesActual = f.Month == FechaCalendario.Month,
                    EsHoy = f.Date == DateTime.Today,
                    TieneInicio = tieneInicio,
                    TieneFin = tieneFin
                });
            }
        }

        public TareaModel? GetProximaTareaAFinalizar()
        {
            return Tasks?
                .Where(t => t.TAR_FEC_FIN.HasValue &&
                            t.TAR_FEC_FIN.Value >= DateTimeOffset.Now &&
                            t.TAR_EST == "Pendiente")
                .OrderBy(t => t.TAR_FEC_FIN)
                .FirstOrDefault();
        }

        public TareaModel? GetProximaTarea() =>
            Tasks?.Where(t => t.TAR_FEC_FIN >= DateTimeOffset.Now)
                  .OrderBy(t => t.TAR_FEC_FIN)
                  .FirstOrDefault();

        // MODALES
        private void AbrirModalCalendario()
        {
            ModalActual = "CALENDARIO";
            MostrarModal = true;
        }

        private void AbrirDetalleTarea(TareaModel t)
        {
            TareaSeleccionada = t;
            ModalActual = "DETALLE";
            MostrarModal = true;
        }

        private void CerrarModal()
        {
            MostrarModal = false;
            ModalActual = "";
            TareaSeleccionada = null;
        }

        // 🔥 NUEVO: BITÁCORA DE TAREAS
        private void AbrirBitacoraTarea()
        {
            MostrarBitacoraTarea = true;
            Console.WriteLine("📖 Abriendo Bitácora de Tareas");
        }

        private void CerrarBitacoraTarea()
        {
            MostrarBitacoraTarea = false;
            Console.WriteLine("❌ Cerrando Bitácora de Tareas");
        }

        // NAVEGACIÓN CALENDARIO
        public void MesAnterior()
        {
            FechaCalendario = FechaCalendario.AddMonths(-1);
            GenerarCalendarioMini();
        }

        public void MesSiguiente()
        {
            FechaCalendario = FechaCalendario.AddMonths(1);
            GenerarCalendarioMini();
        }

        // CLASE AUXILIAR
        public class CalendarDayAsignacion
        {
            public DateTime Fecha { get; set; }
            public bool EsMesActual { get; set; }
            public bool EsHoy { get; set; }
            public bool TieneInicio { get; set; }
            public bool TieneFin { get; set; }
        }
    }
}
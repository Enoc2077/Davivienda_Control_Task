using Davivienda.GraphQL.SDK;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Davivienda.FrontEnd.Pages.Pagess
{
    public partial class Asignaciones : IDisposable
    {
        [Inject] private DaviviendaGraphQLClient Client { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

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
        public bool MostrarBitacoraTarea { get; set; }
        public int TotalTareasCount { get; set; }
        public int TotalProyectos { get; set; }

        private const int MaxCompletadasVisibles = 5;
        private const int HorasPermitidas = 24;

        private DotNetObjectReference<Asignaciones>? _dotNetRef;

        protected override async Task OnInitializedAsync()
        {
            await CargarDatosDesdeBase();
            GenerarCalendarioMini();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("registrarInstanciaAsignaciones", _dotNetRef);
            }
        }

        [JSInvokable]
        public void FiltrarDesdeGrafica(string estado)
        {
            Console.WriteLine($"Click en barra: {estado}");
            FiltrarPorEstadoGrafica(estado);
            StateHasChanged();
        }

        // Reemplaza el metodo RenderizarGrafica en Asignaciones.razor.cs

        private async Task RenderizarGrafica()
        {
            try
            {
                // Contar las tareas que SE VEN en la interfaz (las que pasaron AplicarReglasHistorial)
                int pendientes = Tasks?.Count(t => t.TAR_EST == "Pendiente") ?? 0;
                int enProgreso = Tasks?.Count(t => t.TAR_EST == "En Progreso") ?? 0;
                // Completadas que aun estan visibles (dentro de las 24h, maximo 5)
                int completadas = Tasks?.Count(t => t.TAR_EST == "Completado") ?? 0;

                Console.WriteLine($"Grafica: P={pendientes} EP={enProgreso} C={completadas}");

                await JS.InvokeVoidAsync("renderEstadoTareasChart",
                    "estadoTareasChart",
                    pendientes,
                    enProgreso,
                    completadas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Chart.js: {ex.Message}");
            }
        }

        private async Task CargarDatosDesdeBase()
        {
            try
            {
                var authState = await AuthProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                UserRole = user.FindFirst(ClaimTypes.Role)?.Value
                           ?? user.FindFirst("role")?.Value
                           ?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                           ?? "Empleado";

                Console.WriteLine("========================================");
                Console.WriteLine($"ROL DETECTADO: {UserRole}");

                var usuNumClaim = user.FindFirst("USU_NUM")?.Value ?? "";
                if (string.IsNullOrEmpty(usuNumClaim))
                {
                    Console.WriteLine("No se encontro el claim USU_NUM");
                    return;
                }

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
                    Console.WriteLine($"Usuario: {UsuarioActual.USU_NOM} | Area: {UserAreaId}");
                }
                else
                {
                    Console.WriteLine("Usuario no encontrado");
                    return;
                }

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
                    PROC_ID = t.Proc_ID,
                    TAR_FEC_CRE = t.Tar_FEC_CRE,
                    TAR_FEC_MOD = t.Tar_FEC_MOD
                }).ToList() ?? new();

                Console.WriteLine($"Total tareas en BD: {allTasks.Count}");

                var resProc = await Client.GetProcesos.ExecuteAsync();
                var procesosActivos = resProc.Data?.Procesos
                    .Where(p => p.Proc_EST == true)
                    .Select(p => p.Proc_ID)
                    .ToList() ?? new();

                Console.WriteLine($"Procesos activos: {procesosActivos.Count}");

                allTasks = allTasks.Where(t =>
                {
                    if (!t.PROC_ID.HasValue) return true;
                    return procesosActivos.Contains(t.PROC_ID.Value);
                }).ToList();

                Console.WriteLine($"Tareas con proceso activo: {allTasks.Count}");

                Tasks = await FiltrarTareasPorRolYArea(allTasks);
                Console.WriteLine($"Tareas visibles para {UserRole}: {Tasks.Count}");

                AplicarReglasHistorial();
                Console.WriteLine($"Tras reglas historial: {Tasks.Count}");
                Console.WriteLine("========================================");

                // FIX: TareasFiltradas incluye TODAS (pendiente, en progreso
                // y completadas dentro de las 24h que aun deben verse)
                TareasFiltradas = Tasks.ToList();

                // El contador solo muestra las no completadas (activas)
                TotalTareasCount = Tasks.Count(t => t.TAR_EST != "Completado");

                var resProy = await Client.GetProyectos.ExecuteAsync();
                ProyectosRelacionados = resProy.Data?.Proyectos
                    .Select(p => new ProyectosModel
                    {
                        PRO_ID = p.Pro_ID,
                        PRO_NOM = p.Pro_NOM,
                        ARE_ID = p.Are_ID
                    }).ToList() ?? new();
                TotalProyectos = ProyectosRelacionados.Count;

                var resPrio = await Client.GetPrioridades.ExecuteAsync();
                Prioridades = resPrio.Data?.Prioridades
                    .Select(p => new PrioridadModel { PRI_ID = p.Pri_ID, PRI_NOM = p.Pri_NOM })
                    .ToList();

                StateHasChanged();
                await Task.Delay(300);
                await RenderizarGrafica();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN DASHBOARD: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }
        }

        // ============================================================
        // REGLAS DE HISTORIAL:
        // Regla 1: Completadas con mas de 24h → historial
        // Regla 2: Si hay mas de 5 completadas dentro de 24h → las mas antiguas al historial
        // Regla 3: Si TODAS las del proceso estan completadas Y todas >24h → historial
        //          Si alguna tiene <24h → permanecen visibles hasta cumplir las 24h
        // ============================================================
        private void AplicarReglasHistorial()
        {
            if (Tasks == null || !Tasks.Any()) return;

            var ahora = DateTimeOffset.Now;
            var tareasBorrar = new HashSet<Guid>();

            // Regla 1: Completadas con mas de 24h → historial
            var completadasViejas = Tasks
                .Where(t => t.TAR_EST == "Completado")
                .Where(t =>
                {
                    var fecha = t.TAR_FEC_MOD ?? t.TAR_FEC_CRE;
                    if (fecha == default) return false;
                    return (ahora - fecha).TotalHours > HorasPermitidas;
                })
                .ToList();

            foreach (var t in completadasViejas)
            {
                tareasBorrar.Add(t.TAR_ID);
                Console.WriteLine($"Historial por 24h: {t.TAR_NOM}");
            }

            // Regla 2: Si quedan mas de 5 completadas visibles (dentro de 24h)
            // las mas antiguas van al historial
            var completadasRestantes = Tasks
                .Where(t => t.TAR_EST == "Completado" && !tareasBorrar.Contains(t.TAR_ID))
                .OrderBy(t => t.TAR_FEC_MOD ?? t.TAR_FEC_CRE)
                .ToList();

            if (completadasRestantes.Count > MaxCompletadasVisibles)
            {
                int exceso = completadasRestantes.Count - MaxCompletadasVisibles;
                foreach (var t in completadasRestantes.Take(exceso))
                {
                    tareasBorrar.Add(t.TAR_ID);
                    Console.WriteLine($"Historial por limite: {t.TAR_NOM}");
                }
            }

            // Regla 3: Si TODAS las tareas de un proceso estan completadas
            // solo van al historial si TODAS tienen mas de 24h
            var procesosIds = Tasks
                .Where(t => t.PROC_ID.HasValue)
                .Select(t => t.PROC_ID!.Value)
                .Distinct()
                .ToList();

            foreach (var procId in procesosIds)
            {
                var tareasDelProceso = Tasks.Where(t => t.PROC_ID == procId).ToList();

                if (tareasDelProceso.All(t => t.TAR_EST == "Completado"))
                {
                    bool todasSuperaron24h = tareasDelProceso.All(t =>
                    {
                        var fecha = t.TAR_FEC_MOD ?? t.TAR_FEC_CRE;
                        if (fecha == default) return false;
                        return (ahora - fecha).TotalHours > HorasPermitidas;
                    });

                    if (todasSuperaron24h)
                    {
                        foreach (var t in tareasDelProceso)
                            tareasBorrar.Add(t.TAR_ID);
                        Console.WriteLine($"Proceso {procId}: todas completadas y >24h → historial");
                    }
                    else
                    {
                        Console.WriteLine($"Proceso {procId}: todas completadas pero <24h → permanecen visibles");
                    }
                }
            }

            if (tareasBorrar.Any())
            {
                Tasks = Tasks.Where(t => !tareasBorrar.Contains(t.TAR_ID)).ToList();
                Console.WriteLine($"Total movidas al historial: {tareasBorrar.Count}");
            }
        }

        private async Task<List<TareaModel>> FiltrarTareasPorRolYArea(List<TareaModel> todasLasTareas)
        {
            if (EsGerente(UserRole))
            {
                Console.WriteLine("GERENTE/ADMIN: Acceso total");
                return todasLasTareas;
            }

            if (EsLiderTecnico(UserRole))
            {
                Console.WriteLine($"LIDER TECNICO: area {UserAreaId}");
                if (!UserAreaId.HasValue) return new List<TareaModel>();

                var filtradas = new List<TareaModel>();
                foreach (var tarea in todasLasTareas)
                {
                    if (tarea.PROC_ID.HasValue)
                    {
                        try
                        {
                            var resProceso = await Client.GetProcesoById.ExecuteAsync(tarea.PROC_ID.Value);
                            if (resProceso.Data?.ProcesoById?.Pro_ID != null)
                            {
                                var proyId = resProceso.Data.ProcesoById.Pro_ID.Value;
                                var resProyecto = await Client.GetProyectoById.ExecuteAsync(proyId);
                                if (resProyecto.Data?.ProyectoById?.Are_ID == UserAreaId)
                                    filtradas.Add(tarea);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error tarea '{tarea.TAR_NOM}': {ex.Message}");
                        }
                    }
                }
                Console.WriteLine($"  Total tareas del area: {filtradas.Count}");
                return filtradas;
            }

            Console.WriteLine("EMPLEADO: Solo tareas asignadas");
            var tareasEmpleado = todasLasTareas
                .Where(t => t.USU_ID == UsuarioActual?.USU_ID)
                .ToList();
            Console.WriteLine($"  {tareasEmpleado.Count} tareas asignadas");
            return tareasEmpleado;
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

        private void FiltrarPorPrioridad(string prio)
        {
            PrioridadSeleccionada = prio;
            // FIX: base incluye TODAS las tareas (incluyendo completadas dentro de 24h)
            TareasFiltradas = prio == "Todas"
                ? Tasks!.ToList()
                : Tasks!.Where(t => GetPrioridadNombre(t.PRI_ID) == prio).ToList();
        }

        private void FiltrarPorEstadoGrafica(string estado)
        {
            PrioridadSeleccionada = "Todas";
            // FIX: base incluye TODAS, filtro por estado si se pide
            TareasFiltradas = estado == "Todas"
                ? Tasks!.ToList()
                : Tasks!.Where(t => t.TAR_EST == estado).ToList();
        }

        public string GetPrioridadNombre(Guid? id) =>
            Prioridades?.FirstOrDefault(p => p.PRI_ID == id)?.PRI_NOM ?? "Baja";

        public void GenerarCalendarioMini()
        {
            DiasDelMesAsignaciones.Clear();
            var primeroMes = new DateTime(FechaCalendario.Year, FechaCalendario.Month, 1);
            int offset = (int)primeroMes.DayOfWeek;
            var fechaInicio = primeroMes.AddDays(-offset);

            // FIX: todas las tareas visibles, sin filtrar por estado
            var todasLasTareas = Tasks ?? new();

            for (int i = 0; i < 35; i++)
            {
                var f = fechaInicio.AddDays(i);

                // Verde = tareas que INICIAN ese dia (cualquier estado)
                var tInicio = todasLasTareas
                    .Where(t => t.TAR_FEC_INI.Date == f.Date)
                    .ToList();

                // Rojo = tareas que FINALIZAN ese dia (cualquier estado)
                var tFin = todasLasTareas
                    .Where(t => t.TAR_FEC_FIN.HasValue && t.TAR_FEC_FIN.Value.Date == f.Date)
                    .ToList();

                string tooltip = "";
                if (tInicio.Any()) tooltip += "Inicio: " + string.Join(", ", tInicio.Select(t => t.TAR_NOM)) + " ";
                if (tFin.Any()) tooltip += "Fin: " + string.Join(", ", tFin.Select(t => t.TAR_NOM));

                DiasDelMesAsignaciones.Add(new CalendarDayAsignacion
                {
                    Fecha = f,
                    EsMesActual = f.Month == FechaCalendario.Month,
                    EsHoy = f.Date == DateTime.Today,
                    TieneInicio = tInicio.Any(),
                    TieneFin = tFin.Any(),
                    Tooltip = tooltip.Trim()
                });
            }
        }

        public TareaModel? GetProximaTareaAFinalizar() =>
            Tasks?.Where(t => t.TAR_FEC_FIN.HasValue &&
                              t.TAR_FEC_FIN.Value >= DateTimeOffset.Now &&
                              t.TAR_EST == "Pendiente")
                  .OrderBy(t => t.TAR_FEC_FIN)
                  .FirstOrDefault();

        public TareaModel? GetProximaTarea() =>
            Tasks?.Where(t => t.TAR_FEC_FIN >= DateTimeOffset.Now)
                  .OrderBy(t => t.TAR_FEC_FIN)
                  .FirstOrDefault();

        private void AbrirModalCalendario() { ModalActual = "CALENDARIO"; MostrarModal = true; }

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

        private void AbrirBitacoraTarea()
        {
            MostrarBitacoraTarea = true;
            Console.WriteLine("Abriendo Bitacora de Tareas");
        }

        private void CerrarBitacoraTarea()
        {
            MostrarBitacoraTarea = false;
            Console.WriteLine("Cerrando Bitacora de Tareas");
        }

        public void MesAnterior() { FechaCalendario = FechaCalendario.AddMonths(-1); GenerarCalendarioMini(); }
        public void MesSiguiente() { FechaCalendario = FechaCalendario.AddMonths(1); GenerarCalendarioMini(); }

        public void Dispose() => _dotNetRef?.Dispose();

        public class CalendarDayAsignacion
        {
            public DateTime Fecha { get; set; }
            public bool EsMesActual { get; set; }
            public bool EsHoy { get; set; }
            public bool TieneInicio { get; set; }
            public bool TieneFin { get; set; }
            public string Tooltip { get; set; } = "";
        }
    }
}
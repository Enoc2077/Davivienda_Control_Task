using Davivienda.Models;
using Davivienda.Models.Modelos;
using HotChocolate;
using HotChocolate.Types;

namespace Davivienda.GraphQL.ServicesQuery.Type
{
    public class Suscripciones
    {
        // --- AREAS ---
        [Subscribe]
        [Topic]
        public AreasModel OnAreaInserted([EventMessage] AreasModel area) => area;
        [Subscribe]
        [Topic]
        public AreasModel OnAreaUpdated([EventMessage] AreasModel area) => area;

        // --- PROYECTOS ---
        [Subscribe]
        [Topic]
        public ProyectosModel OnProyectoInserted([EventMessage] ProyectosModel proyecto) => proyecto;
        [Subscribe]
        [Topic]
        public ProyectosModel OnProyectoUpdated([EventMessage] ProyectosModel proyecto) => proyecto;

        // --- DETALLE PROYECTO ---
        [Subscribe]
        [Topic]
        public DetalleProyectoModel OnDetalleProyectoInserted([EventMessage] DetalleProyectoModel detalle) => detalle;
        [Subscribe]
        [Topic]
        public DetalleProyectoModel OnDetalleProyectoUpdated([EventMessage] DetalleProyectoModel detalle) => detalle;

        // --- TAREAS ---
        [Subscribe]
        [Topic]
        public TareaModel OnTareaInserted([EventMessage] TareaModel tarea) => tarea;
        [Subscribe]
        [Topic]
        public TareaModel OnTareaUpdated([EventMessage] TareaModel tarea) => tarea;

        // --- USUARIOS Y ROLES ---
        [Subscribe]
        [Topic]
        public UsuarioModel OnUsuarioInserted([EventMessage] UsuarioModel usuario) => usuario;
        [Subscribe]
        [Topic]
        public RolesModel OnRolInserted([EventMessage] RolesModel rol) => rol;

        // --- FRICCIONES Y SOLUCIONES ---
        [Subscribe]
        [Topic]
        public FriccionModel OnFriccionInserted([EventMessage] FriccionModel friccion) => friccion;
        [Subscribe]
        [Topic]
        public FriccionModel OnFriccionUpdated([EventMessage] FriccionModel friccion) => friccion;

        [Subscribe]
        [Topic]
        public SolucionesModel OnSolucionInserted([EventMessage] SolucionesModel solucion) => solucion;
        [Subscribe]
        [Topic]
        public SolucionesModel OnSolucionUpdated([EventMessage] SolucionesModel solucion) => solucion;

        // --- BITÁCORAS ---
        [Subscribe]
        [Topic]
        public BitacoraFriccionModel OnBitacoraFriccionInserted([EventMessage] BitacoraFriccionModel bitacora) => bitacora;

        [Subscribe]
        [Topic]
        public BitacoraSolucionesModel OnBitacoraSolucionInserted([EventMessage] BitacoraSolucionesModel bitacora) => bitacora;

        // --- COMENTARIOS Y DOCUMENTACIÓN ---
        [Subscribe]
        [Topic]
        public ComentariosModel OnComentarioInserted([EventMessage] ComentariosModel comentario) => comentario;

        [Subscribe]
        [Topic]
        public DocumentacionModel OnDocumentacionInserted([EventMessage] DocumentacionModel doc) => doc;

        // --- NOTIFICACIONES Y PRIORIDADES ---
        [Subscribe]
        [Topic]
        public NotificacionesModel OnNotificacionSent([EventMessage] NotificacionesModel notificacion) => notificacion;

        [Subscribe]
        [Topic]
        public PrioridadModel OnPrioridadUpdated([EventMessage] PrioridadModel prioridad) => prioridad;

        // --- PROCESOS ---
        [Subscribe]
        [Topic]
        public ProcesoModel OnProcesoInserted([EventMessage] ProcesoModel proceso) => proceso;
    }
}    //damelos completos
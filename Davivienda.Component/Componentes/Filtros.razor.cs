using Davivienda.Models;
using Davivienda.Models.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class Filtros
    {
        [Parameter] public List<AreasModel> Areas { get; set; } = new();
        [Parameter] public List<ProyectosModel> Proyectos { get; set; } = new();
        [Parameter] public List<ProcesoModel> Procesos { get; set; } = new();
        [Parameter] public List<TareaModel> Tareas { get; set; } = new();
        [Parameter] public List<PrioridadModel> Prioridades { get; set; } = new();
        [Parameter] public EventCallback<List<FiltroActivoModel>> OnFiltrosChanged { get; set; }

        private List<FiltroActivoModel> _filtrosActivos = new();
        private string _tipoActual = string.Empty;
        private List<OpcionInterna> _opcionesActuales = new();
        private string _textoNombre = string.Empty;

        private record OpcionInterna(Guid Id, string Etiqueta);

        private void OnTipoChanged(ChangeEventArgs e)
        {
            _tipoActual = e.Value?.ToString() ?? string.Empty;
            _textoNombre = string.Empty;
            _opcionesActuales = _tipoActual switch
            {
                "Area" => Areas.Select(a => new OpcionInterna(a.ARE_ID, a.ARE_NOM ?? "Sin nombre")).ToList(),
                "Proyecto" => Proyectos.Select(p => new OpcionInterna(p.PRO_ID, p.PRO_NOM)).ToList(),
                "Proceso" => Procesos.Select(p => new OpcionInterna(p.PROC_ID, p.PROC_NOM)).ToList(),
                "Tarea" => Tareas.Select(t => new OpcionInterna(t.TAR_ID, t.TAR_NOM)).ToList(),
                "Prioridad" => Prioridades.Select(p => new OpcionInterna(p.PRI_ID, p.PRI_NOM ?? "Sin nombre")).ToList(),

                // NUEVO: Año — años disponibles en los filtros activos + años del sistema
                "Año" => Enumerable.Range(DateTime.Today.Year - 4, 6)
                    .Select(y => new OpcionInterna(Guid.NewGuid(), y.ToString()))
                    .ToList(),

                // NUEVO: Mes — 12 meses
                "Mes" => Enumerable.Range(1, 12)
                    .Select(m => new OpcionInterna(Guid.NewGuid(),
                        new DateTime(2000, m, 1).ToString("MMMM",
                            new System.Globalization.CultureInfo("es-ES"))))
                    .ToList(),

                // NUEVO: Dia — 1 al 31
                "Dia" => Enumerable.Range(1, 31)
                    .Select(d => new OpcionInterna(Guid.NewGuid(), d.ToString()))
                    .ToList(),

                _ => new()
            };
        }

        private async Task OnValorSelected(ChangeEventArgs e)
        {
            var valorStr = e.Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(valorStr)) return;

            // Para Año, Mes y Dia el valor es texto, no Guid
            if (_tipoActual == "Año" || _tipoActual == "Mes" || _tipoActual == "Dia")
            {
                // Evitar duplicados del mismo tipo+etiqueta
                if (_filtrosActivos.Any(f => f.Tipo == _tipoActual && f.Etiqueta == valorStr)) return;

                _filtrosActivos.Add(new FiltroActivoModel
                {
                    Tipo = _tipoActual,
                    Id = null,
                    Etiqueta = valorStr,
                    Icono = ResolverIcono(_tipoActual),
                    ColorClase = ResolverColor(_tipoActual)
                });

                _tipoActual = string.Empty;
                await NotificarCambio();
                return;
            }

            // Para el resto — valor es un Guid
            if (!Guid.TryParse(valorStr, out Guid id)) return;
            var opcion = _opcionesActuales.FirstOrDefault(o => o.Id == id);
            if (opcion is null) return;
            if (_filtrosActivos.Any(f => f.Tipo == _tipoActual && f.Id == id)) return;

            _filtrosActivos.Add(new FiltroActivoModel
            {
                Tipo = _tipoActual,
                Id = id,
                Etiqueta = opcion.Etiqueta,
                Icono = ResolverIcono(_tipoActual),
                ColorClase = ResolverColor(_tipoActual)
            });

            _tipoActual = string.Empty;
            await NotificarCambio();
        }

        private async Task AgregarFiltroNombre()
        {
            var texto = _textoNombre.Trim();
            if (string.IsNullOrEmpty(texto)) return;
            if (_filtrosActivos.Any(f => f.Tipo == "Nombre" &&
                f.Etiqueta.Equals(texto, StringComparison.OrdinalIgnoreCase))) return;

            _filtrosActivos.Add(new FiltroActivoModel
            {
                Tipo = "Nombre",
                Id = null,
                Etiqueta = texto,
                Icono = "bi-search",
                ColorClase = "gris"
            });

            _textoNombre = string.Empty;
            _tipoActual = string.Empty;
            await NotificarCambio();
        }

        private async Task OnNombreKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await AgregarFiltroNombre();
        }

        private async Task RemoverFiltro(FiltroActivoModel filtro)
        {
            _filtrosActivos.RemoveAll(f => f.UniqueKey == filtro.UniqueKey);
            await NotificarCambio();
        }

        private async Task LimpiarTodo()
        {
            _filtrosActivos.Clear();
            _tipoActual = string.Empty;
            await NotificarCambio();
        }

        private async Task NotificarCambio() =>
            await OnFiltrosChanged.InvokeAsync(_filtrosActivos.ToList());

        private static string ResolverIcono(string tipo) => tipo switch
        {
            "Area" => "bi-building",
            "Proyecto" => "bi-folder2-open",
            "Proceso" => "bi-gear",
            "Tarea" => "bi-list-check",
            "Prioridad" => "bi-flag-fill",
            "Nombre" => "bi-search",
            "Año" => "bi-calendar4",
            "Mes" => "bi-calendar-month",
            "Dia" => "bi-calendar-day",
            _ => "bi-tag"
        };

        private static string ResolverColor(string tipo) => tipo switch
        {
            "Area" => "morado",
            "Proyecto" => "azul",
            "Proceso" => "verde",
            "Tarea" => "naranja",
            "Prioridad" => "rojo",
            "Año" => "cyan",
            "Mes" => "cyan",
            "Dia" => "cyan",
            _ => "gris"
        };
    }
}
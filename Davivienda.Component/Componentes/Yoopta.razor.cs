using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Davivienda.Component.Componentes
{
    public partial class Yoopta : ComponentBase
    {
        [Parameter] public string? Contenido { get; set; }
        [Parameter] public EventCallback<string> ContenidoChanged { get; set; }

        private List<LineaYoopta> Lineas { get; set; } = new();
        private LineaYoopta? lineaActiva;
        private bool mostrarMenu = false;

        protected override void OnInitialized()
        {
            if (!string.IsNullOrEmpty(Contenido))
            {
                CargarContenido();
            }
            else
            {
                Lineas.Add(new LineaYoopta
                {
                    Id = Guid.NewGuid(),
                    Tipo = TipoLinea.Texto,
                    Contenido = ""
                });
            }
        }

        private void CargarContenido()
        {
            try
            {
                var lineasCargadas = JsonSerializer.Deserialize<List<LineaYoopta>>(Contenido!);
                if (lineasCargadas != null && lineasCargadas.Any())
                {
                    Lineas = lineasCargadas;
                }
            }
            catch
            {
                Lineas = new List<LineaYoopta>
                {
                    new LineaYoopta
                    {
                        Id = Guid.NewGuid(),
                        Tipo = TipoLinea.Texto,
                        Contenido = Contenido ?? ""
                    }
                };
            }
        }

        private void SeleccionarLinea(LineaYoopta linea)
        {
            lineaActiva = linea;
            mostrarMenu = false;
            StateHasChanged();
        }

        private void MostrarMenuLinea(LineaYoopta linea)
        {
            lineaActiva = linea;
            mostrarMenu = !mostrarMenu;
            StateHasChanged();
        }

        private async Task OnLineaChange(LineaYoopta linea, ChangeEventArgs e)
        {
            var texto = e.Value?.ToString() ?? "";
            linea.Contenido = texto;

            if (texto.EndsWith("/"))
            {
                lineaActiva = linea;
                mostrarMenu = true;
                StateHasChanged();
            }
            else
            {
                mostrarMenu = false;
            }

            await GuardarContenido();
        }

        private async Task CambiarTipoLinea(TipoLinea nuevoTipo)
        {
            if (lineaActiva == null) return;

            // Limpiar "/" si existe
            if (lineaActiva.Contenido.EndsWith("/"))
            {
                lineaActiva.Contenido = lineaActiva.Contenido.TrimEnd('/');
            }

            var index = Lineas.IndexOf(lineaActiva);

            if (nuevoTipo == TipoLinea.Imagen || nuevoTipo == TipoLinea.Documento)
            {
                // NO convertir el renglón actual, crear uno nuevo abajo
                // Renglón para el archivo
                var nuevoRenglonArchivo = new LineaYoopta
                {
                    Id = Guid.NewGuid(),
                    Tipo = nuevoTipo,
                    Contenido = ""
                };

                // Renglón para seguir escribiendo
                var nuevoRenglonTexto = new LineaYoopta
                {
                    Id = Guid.NewGuid(),
                    Tipo = TipoLinea.Texto,
                    Contenido = ""
                };

                // Insertar ambos renglones abajo del actual
                Lineas.Insert(index + 1, nuevoRenglonArchivo);
                Lineas.Insert(index + 2, nuevoRenglonTexto);
            }
            else
            {
                // Para tipo Texto, solo convertir el actual
                lineaActiva.Tipo = nuevoTipo;
            }

            mostrarMenu = false;
            await GuardarContenido();
            StateHasChanged();
        }

        private async Task OnArchivoSeleccionado(LineaYoopta linea, InputFileChangeEventArgs e)
        {
            var archivo = e.File;
            if (archivo != null)
            {
                linea.NombreArchivo = archivo.Name;

                using var stream = archivo.OpenReadStream(maxAllowedSize: 10485760);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();
                var base64 = Convert.ToBase64String(bytes);

                if (linea.Tipo == TipoLinea.Imagen)
                {
                    linea.Contenido = $"data:{archivo.ContentType};base64,{base64}";
                }
                else if (linea.Tipo == TipoLinea.Documento)
                {
                    linea.Contenido = base64;
                    linea.TipoMime = archivo.ContentType;
                }

                var index = Lineas.IndexOf(linea);
                Lineas.Insert(index + 1, new LineaYoopta
                {
                    Id = Guid.NewGuid(),
                    Tipo = TipoLinea.Texto,
                    Contenido = ""
                });

                await GuardarContenido();
            }
        }

        private async Task EliminarLinea(LineaYoopta linea)
        {
            if (Lineas.Count > 1)
            {
                Lineas.Remove(linea);
                await GuardarContenido();
                StateHasChanged();
            }
        }

        private async Task GuardarContenido()
        {
            var json = JsonSerializer.Serialize(Lineas);
            await ContenidoChanged.InvokeAsync(json);
            StateHasChanged();
        }

        public class LineaYoopta
        {
            public Guid Id { get; set; }
            public TipoLinea Tipo { get; set; }
            public string Contenido { get; set; } = "";
            public string NombreArchivo { get; set; } = "";
            public string TipoMime { get; set; } = "";
        }

        public enum TipoLinea
        {
            Texto,
            Imagen,
            Documento
        }
    }
}

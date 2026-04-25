using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
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
        [Inject] private IJSRuntime js { get; set; } = default!;

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
                var nuevoRenglonArchivo = new LineaYoopta
                {
                    Id = Guid.NewGuid(),
                    Tipo = nuevoTipo,
                    Contenido = ""
                };

                var nuevoRenglonTexto = new LineaYoopta
                {
                    Id = Guid.NewGuid(),
                    Tipo = TipoLinea.Texto,
                    Contenido = ""
                };

                Lineas.Insert(index + 1, nuevoRenglonArchivo);
                Lineas.Insert(index + 2, nuevoRenglonTexto);
            }
            else
            {
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
                    linea.TipoMime = archivo.ContentType;
                }
                else if (linea.Tipo == TipoLinea.Documento)
                {
                    // Guardamos el base64 puro y el mime por separado
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

        // ✅ DESCARGA REAL DE DOCUMENTOS vía JS
        private async Task DescargarDocumento(LineaYoopta linea)
        {
            try
            {
                if (string.IsNullOrEmpty(linea.Contenido)) return;

                var mimeType = string.IsNullOrEmpty(linea.TipoMime)
                    ? "application/octet-stream"
                    : linea.TipoMime;

                var dataUrl = $"data:{mimeType};base64,{linea.Contenido}";
                var nombreArchivo = string.IsNullOrEmpty(linea.NombreArchivo)
                    ? $"documento_{DateTime.Now:yyyyMMddHHmmss}"
                    : linea.NombreArchivo;

                await js.InvokeVoidAsync("descargarArchivo", dataUrl, nombreArchivo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al descargar documento: {ex.Message}");
            }
        }

        // ✅ DESCARGA REAL DE IMÁGENES vía JS
        private async Task DescargarImagen(LineaYoopta linea)
        {
            try
            {
                if (string.IsNullOrEmpty(linea.Contenido)) return;

                var nombreArchivo = string.IsNullOrEmpty(linea.NombreArchivo)
                    ? $"imagen_{DateTime.Now:yyyyMMddHHmmss}.png"
                    : linea.NombreArchivo;

                // La imagen ya viene como data URL completo
                await js.InvokeVoidAsync("descargarArchivo", linea.Contenido, nombreArchivo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al descargar imagen: {ex.Message}");
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
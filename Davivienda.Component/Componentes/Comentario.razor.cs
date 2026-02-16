////using Microsoft.AspNetCore.Components;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Davivienda.GraphQL.SDK;

//namespace Davivienda.Component.Componentes
//{
//    public partial class Comentario
//    {
//        [Inject] public DaviviendaGraphQLClient Client { get; set; } = default!;
//        [Parameter] public Guid TareaId { get; set; }
//        [Parameter] public string DescripcionActual { get; set; }

//        private List<BloqueEditor> Bloques = new();
//        private Guid? BloqueActivoId;

//        protected override void OnInitialized()
//        {
//            // Inicializamos con el contenido actual de la tarea si existe
//            Bloques.Add(new BloqueEditor
//            {
//                Tipo = "TEXTO",
//                Contenido = DescripcionActual,
//                Id = Guid.NewGuid()
//            });
//        }

//        private void DetectarSlash(ChangeEventArgs e, BloqueEditor bloque)
//        {
//            if (e.Value?.ToString()?.EndsWith("/") ?? false)
//            {
//                BloqueActivoId = bloque.Id;
//            }
//        }

//        private async Task GuardarTodo()
//        {
//            string textoCompleto = "";
//            foreach (var b in Bloques)
//            {
//                if (b.Tipo == "TEXTO")
//                {
//                    textoCompleto += b.Contenido + " ";
//                    // Guardar en tabla COMENTARIOS
//                    await Client.InsertComentario.ExecuteAsync(new ComentariosModelInput
//                    {
//                        Com_COM = b.Contenido,
//                        tar_ID = TareaId,
//                        Usu_ID = Guid.Parse("0BC4DB21-1FFB-46BB-B120-48AE7B0909CD")
//                    });
//                }
//                else
//                {
//                    // Guardar en tabla DOCUMENTACION
//                    await Client.InsertDocumentacion.ExecuteAsync(new DocumentacionModelInput
//                    {
//                        Doc_NOM = "Archivo Tarea",
//                        Doc_RUT = b.Url,
//                        Tar_ID = TareaId
//                    });
//                }
//            }

//            // ACTUALIZAR DESCRIPCIÓN DE LA TAREA
//            // Aquí llamarías a tu Mutation de UpdateTarea enviando 'textoCompleto'
//        }

//        public class BloqueEditor
//        {
//            public Guid Id { get; set; }
//            public string Tipo { get; set; } = "TEXTO";
//            public string Contenido { get; set; }
//            public string Url { get; set; }
//            public int Lineas => string.IsNullOrEmpty(Contenido) ? 1 : Contenido.Split('\n').Length;
//        }
//    }
//}
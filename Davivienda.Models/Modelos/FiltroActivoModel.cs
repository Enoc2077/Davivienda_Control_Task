using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class FiltroActivoModel
    {
        public Guid UniqueKey { get; set; } = Guid.NewGuid();
        public string Tipo { get; set; } = string.Empty;
        public Guid? Id { get; set; }
        public string Etiqueta { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string ColorClase { get; set; } = string.Empty;
    }
}

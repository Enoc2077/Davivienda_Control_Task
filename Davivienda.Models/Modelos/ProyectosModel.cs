using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class ProyectosModel
    {
        public Guid PRO_ID { get; set; }
        public string PRO_NOM { get; set; } = string.Empty;
        public string? PRO_DES { get; set; }
        public DateTimeOffset PRO_FEC_INI { get; set; }
        public DateTimeOffset? PRO_FEC_FIN { get; set; }
        public string? PRO_EST { get; set; }
        public Guid? ARE_ID { get; set; }
        public DateTimeOffset PRO_FEC_CRE { get; set; }
        public DateTimeOffset? PRO_FEC_MOD { get; set; }
    }
}

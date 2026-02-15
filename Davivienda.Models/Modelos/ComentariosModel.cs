using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class ComentariosModel
    {
        public Guid COM_ID { get; set; }
        public string COM_COM { get; set; } = string.Empty;
        public Guid? FRI_ID { get; set; }
        public Guid? USU_ID { get; set; }
        public DateTimeOffset COM_FEC_CRE { get; set; }
        public DateTimeOffset? COM_FEC_MOD { get; set; }
    }
}

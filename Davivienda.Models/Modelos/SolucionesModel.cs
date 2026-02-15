using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class SolucionesModel
    {
        public Guid SOL_ID { get; set; }
        public string SOL_NOM { get; set; } = string.Empty;
        public string SOL_DES { get; set; } = string.Empty;
        public string SOL_EST { get; set; } = string.Empty;
        public DateTimeOffset? SOL_TIE_RES { get; set; }
        public int? SOL_NIV_EFE { get; set; }
        public Guid? FRI_ID { get; set; }
        public Guid? USU_ID { get; set; }
        public DateTimeOffset SOL_FEC_CRE { get; set; }
        public DateTimeOffset? SOL_FEC_MOD { get; set; }
    }
}

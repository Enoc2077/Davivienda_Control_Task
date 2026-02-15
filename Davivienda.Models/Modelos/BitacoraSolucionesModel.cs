using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class BitacoraSolucionesModel
    {
        public Guid BIT_SOL_ID { get; set; }
        public string? BIT_SOL_NOM { get; set; } = string.Empty;
        public string? BIT_SOL_EST { get; set; } 
        public string? BIT_SOL_DES { get; set; }
        public DateTimeOffset BIT_SOL_TIE_TOT_TRA { get; set; }
        public Guid? SOL_ID { get; set; }
        public Guid? USU_ID { get; set; }
        public DateTimeOffset BIT_SOL_FEC_CRE { get; set; }
        public DateTimeOffset? BIT_SOL_FEC_MOD { get; set; }
    }
}

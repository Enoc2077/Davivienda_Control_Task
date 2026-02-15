using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class BitacoraFriccionModel
    {
        public Guid BIT_FRI_ID { get; set; }
        public string BIT_FRI_NOM { get; set; } = string.Empty; 
        public string BIT_FRI_DES { get; set; } = string.Empty;
        public bool BIT_FRI_EST { get; set; }
        public DateTimeOffset BIT_FRI_FEC_CRE { get; set; }
        public DateTimeOffset? BIT_FRI_FEC_MOD { get; set; }
        public Guid? USU_ID { get; set; }
        public Guid? FRI_ID { get; set; }
    }
}

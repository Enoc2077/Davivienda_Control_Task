using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class FriccionModel
    {
        public Guid FRI_ID { get; set; }
        public string? FRI_TIP { get; set; } 
        public string? FRI_DES { get; set; } 
        public string? FRI_EST { get; set; }
        public string? FRI_IMP { get; set; }
        public Guid? TAR_ID { get; set; }
        public Guid? USU_ID { get; set; }
        public DateTimeOffset FRI_FEC_CRE { get; set; }
        public DateTimeOffset? FRI_FEC_MOD { get; set; }
    }
}

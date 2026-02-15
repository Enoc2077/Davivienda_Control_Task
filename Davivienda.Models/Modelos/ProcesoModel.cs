using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class ProcesoModel
    {
        public Guid PROC_ID { get; set; }
        public string PROC_NOM { get; set; } = string.Empty;
        public string? PROC_DES { get; set; }
        public string? PROC_FRE { get; set; }
        public bool? PROC_EST { get; set; }
        public Guid? PRO_ID { get; set; }
        public DateTimeOffset PROC_FEC_CRE { get; set; }
        public DateTimeOffset? PROC_FEC_MOD { get; set; }
    }
}

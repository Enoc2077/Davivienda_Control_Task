using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class PrioridadModel
    {
        public Guid PRI_ID { get; set; }
        public string? PRI_NOM { get; set; } 
        public string? PRI_DES { get; set; } 
        public int PRI_NIV { get; set; }
        public DateTimeOffset PRI_FEC_CRE { get; set; }
        public DateTimeOffset? PRI_FEC_MOD { get; set; }
    }
}

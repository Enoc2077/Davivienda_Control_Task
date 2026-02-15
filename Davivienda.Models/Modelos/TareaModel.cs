using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class TareaModel
    {
        public Guid TAR_ID { get; set; }
        public string TAR_NOM { get; set; } = string.Empty;
        public string? TAR_DES { get; set; }
        public string? TAR_EST { get; set; }
        public DateTimeOffset TAR_FEC_INI { get; set; }
        public DateTimeOffset? TAR_FEC_FIN { get; set; }
        public Guid? PROC_ID { get; set; }
        public Guid? PRI_ID { get; set; }
        public Guid? USU_ID { get; set; }
        public DateTimeOffset TAR_FEC_CRE { get; set; }
        public DateTimeOffset? TAR_FEC_MOD { get; set; }
    }
}

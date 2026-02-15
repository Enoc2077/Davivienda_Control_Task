using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class DocumentacionModel
    {
        public Guid DOC_ID { get; set; }
        public string DOC_NOM { get; set; } = string.Empty;
        public string DOC_RUT { get; set; } = string.Empty;
        public Guid? TAR_ID { get; set; }
        public Guid? USU_ID { get; set; }
        public DateTimeOffset DOC_FEC_CRE { get; set; }
        public DateTimeOffset? DOC_FEC_MOD { get; set; }
    }
}

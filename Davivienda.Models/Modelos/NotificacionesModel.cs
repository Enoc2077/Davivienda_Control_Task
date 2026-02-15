using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class NotificacionesModel
    {
        public Guid NOT_ID { get; set; }
        public string NOT_MEN { get; set; } = string.Empty;
        public bool NOT_LEI { get; set; }
        public Guid? USU_ID { get; set; }
        public DateTimeOffset NOT_FEC_CRE { get; set; }
        public DateTimeOffset? NOT_FEC_MOD { get; set; }
    }
}

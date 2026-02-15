using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class DetalleProyectoModel
    {
        public Guid DET_PRO_ID { get; set; }
        public Guid? PRO_ID { get; set; }
        public Guid? USU_ID { get; set; }
        public Guid? ROL_ID { get; set; }
        public DateTimeOffset DET_PRO_FEC_ASI { get; set; }
        public DateTimeOffset DET_PRO_FEC_CRE { get; set; }
        public DateTimeOffset? DET_PRO_FEC_MOD { get; set; }
    }
}

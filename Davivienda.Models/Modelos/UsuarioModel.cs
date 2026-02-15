using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Davivienda.Models.Modelos
{
    public class UsuarioModel
    {
        public Guid USU_ID { get; set; }
        public string USU_NOM { get; set; } = string.Empty;
        public string? USU_NUM { get; set; } 
        public string? USU_COR { get; set; } 
        public string? USU_CON { get; set; } 
        public string? USU_TEL { get; set; } 
        public bool? USU_EST { get; set; } 
        public Guid? ROL_ID { get; set; }
        public Guid? ARE_ID { get; set; }
        public DateTimeOffset USU_FEC_CRE { get; set; }
        public DateTimeOffset? USU_FEC_MOD { get; set; }
    }
}

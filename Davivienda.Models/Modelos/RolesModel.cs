using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Davivienda.Models
{
    public class RolesModel
    {
        public Guid ROL_ID { get; set; }
        public string ROL_NOM { get; set; } = string.Empty;
        public string? ROL_DES { get; set; }
        public bool ROL_EST { get; set; }
        public DateTimeOffset? ROL_FEC_CRE { get; set; }
        public DateTimeOffset? ROL_FEC_MOD { get; set; }
    }
}

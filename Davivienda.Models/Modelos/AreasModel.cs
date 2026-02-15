using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Davivienda.Models
{
    public class AreasModel
    {
        public Guid ARE_ID { get; set; }
        public string? ARE_NOM { get; set; } = string.Empty;
        public string? ARE_DES { get; set; }
        public bool? ARE_EST { get; set; }
        public DateTimeOffset? ARE_FEC_CRE { get; set; }
        public DateTimeOffset? ARE_FEC_MOD { get; set; }
    }
}
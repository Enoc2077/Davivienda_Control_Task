using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("ROLES")]
    public class Roles
    {
        [Key]
        [Column("ROL_ID")]
        public Guid ROL_ID { get; set; }

        [Required]
        [Column("ROL_NOM")]
        [StringLength(50)]
        public string ROL_NOM { get; set; } = string.Empty;

        [Column("ROL_DES")]
        public string? ROL_DES { get; set; }

        [Required]
        [Column("ROL_EST")]
        public bool ROL_EST { get; set; }

        [Column("ROL_FEC_CRE")]
        public DateTimeOffset ROL_FEC_CRE { get; set; }

        [Column("ROL_FEC_MOD")]
        public DateTimeOffset ROL_FEC_MOD { get; set; }

    }
}
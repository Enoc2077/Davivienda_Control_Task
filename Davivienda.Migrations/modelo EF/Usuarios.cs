using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("USU_ID")]
        public Guid USU_ID { get; set; }

        [Column("USU_NOM")]
        [StringLength(100)]
        public string? USU_NOM { get; set; }

        [Column("USU_NUM")]
        [StringLength(5)]
        public string? USU_NUM { get; set; }

        [Column("USU_COR")]
        [StringLength(100)]
        public string? USU_COR { get; set; }

        [Column("USU_CON")]
        [StringLength(255)]
        public string? USU_CON { get; set; }

        [Column("USU_TEL")]
        [StringLength(8)]
        public string? USU_TEL { get; set; }

        [Column("USU_EST")]
        public bool? USU_EST { get; set; }

        [Column("ROL_ID")]
        public Guid? ROL_ID { get; set; } // Cambiado a Guid para coincidir con la PK de Roles

        [ForeignKey("ROL_ID")]
        public Roles? Rol { get; set; }

        [Column("ARE_ID")]
        public Guid? ARE_ID { get; set; }

        [ForeignKey("ARE_ID")]
        public Area? Area { get; set; }

        [Column("USU_FEC_CRE")]
        public DateTimeOffset? USU_FEC_CRE { get; set; }

        [Column("USU_FEC_MOD")]
        public DateTimeOffset? USU_FEC_MOD { get; set; }
    }
}
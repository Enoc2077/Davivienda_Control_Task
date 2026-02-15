using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("SOLUCIONES")]
    public class Soluciones
    {
        [Key]
        [Column("SOL_ID")]
        public Guid SOL_ID { get; set; }

        [Column("SOL_NOM")]
        [StringLength(100)]
        public string? SOL_NOM { get; set; }

        [Column("SOL_DES")]
        [StringLength(255)]
        public string? SOL_DES { get; set; }

        [Column("SOL_TIE_RES")]
        public DateTimeOffset? SOL_TIE_RES { get; set; } // Representa el momento de resolución

        [Column("SOL_NIV_EFE")]
        public int? SOL_NIV_EFE { get; set; } // Escala numérica de efectividad

        [Column("SOL_EST")]
        [StringLength(50)]
        public string? SOL_EST { get; set; }

        [Column("SOL_FEC_CRE")]
        public DateTimeOffset? SOL_FEC_CRE { get; set; }

        [Column("SOL_FEC_MOD")]
        public DateTimeOffset? SOL_FEC_MOD { get; set; }

        // --- Relaciones ---

        [Column("FRI_ID")]
        public Guid? FRI_ID { get; set; }

        [ForeignKey("FRI_ID")]
        public Friccion? Friccion { get; set; }

        [Column("USU_ID")]
        public Guid? USU_ID { get; set; }

        [ForeignKey("USU_ID")]
        public Usuario? Usuario { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("FRICCION")]
    public class Friccion
    {
        [Key]
        [Column("FRI_ID")]
        public Guid FRI_ID { get; set; }

        [Column("FRI_TIP")]
        [StringLength(50)]
        public string? FRI_TIP { get; set; } // Tipo de fricción (ej. Técnica, Administrativa)

        [Column("FRI_DES")]
        [StringLength(255)]
        public string? FRI_DES { get; set; }

        [Column("FRI_EST")]
        [StringLength(50)]
        public string? FRI_EST { get; set; } // Estado (ej. Abierta, Mitigada, Resuelta)

        [Column("FRI_IMP")]
        [StringLength(50)]
        public string? FRI_IMP { get; set; } // Impacto (ej. Alto, Medio, Bajo)

        [Column("FRI_FEC_CRE")]
        public DateTimeOffset? FRI_FEC_CRE { get; set; }

        [Column("FRI_FEC_MOD")]
        public DateTimeOffset? FRI_FEC_MOD { get; set; }

        // --- Relaciones ---

        [Column("TAR_ID")]
        public Guid? TAR_ID { get; set; }

        [ForeignKey("TAR_ID")]
        public Tarea? Tarea { get; set; }

        [Column("USU_ID")]
        public Guid? USU_ID { get; set; }

        [ForeignKey("USU_ID")]
        public Usuario? Usuario { get; set; }
    }
}
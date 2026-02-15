using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("BITACORA_SOLUCIONES")]
    public class BitacoraSoluciones
    {
        [Key]
        [Column("BIT_SOL_ID")]
        public Guid BIT_SOL_ID { get; set; }

        [Column("BIT_SOL_NOM")]
        [StringLength(100)]
        public string? BIT_SOL_NOM { get; set; }

        [Column("BIT_SOL_EST")]
        [StringLength(50)]
        public string? BIT_SOL_EST { get; set; }

        [Column("BIT_SOL_DES")]
        [StringLength(255)]
        public string? BIT_SOL_DES { get; set; }

        [Column("BIT_SOL_TIE_TOT_TRA")]
        public DateTimeOffset? BIT_SOL_TIE_TOT_TRA { get; set; } // Tiempo total trabajado (en minutos)

        [Column("BIT_SOL_FEC_CRE")]
        public DateTimeOffset? BIT_SOL_FEC_CRE { get; set; }

        [Column("BIT_SOL_FEC_MOD")]
        public DateTimeOffset? BIT_SOL_FEC_MOD { get; set; }

        // --- Relaciones ---

        [Column("SOL_ID")]
        public Guid? SOL_ID { get; set; }

        [ForeignKey("SOL_ID")]
        public Soluciones? Solucion { get; set; }

        [Column("USU_ID")]
        public Guid? USU_ID { get; set; }

        [ForeignKey("USU_ID")]
        public Usuario? Usuario { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("BITACORA_FRICCIONES")]
    public class BitacoraFriccion
    {
        [Key]
        [Column("BIT_FRI_ID")]
        public Guid BIT_FRI_ID { get; set; }

        [Column("BIT_FRI_NOM")]
        [StringLength(100)]
        public string? BIT_FRI_NOM { get; set; }

        [Column("BIT_FRI_DES")]
        [StringLength(255)]
        public string? BIT_FRI_DES { get; set; }

        [Column("BIT_FRI_EST")]
        [StringLength(50)]
        public string? BIT_FRI_EST { get; set; }

        [Column("BIT_FRI_FEC_CRE")]
        public DateTimeOffset? BIT_FRI_FEC_CRE { get; set; }

        [Column("BIT_FRI_FEC_MOD")]
        public DateTimeOffset? BIT_FRI_FEC_MOD { get; set; }

        // --- Relaciones ---

        [Column("USU_ID")]
        public Guid? USU_ID { get; set; }

        [ForeignKey("USU_ID")]
        public Usuario? Usuario { get; set; }

        [Column("FRI_ID")]
        public Guid? FRI_ID { get; set; }

        [ForeignKey("FRI_ID")]
        public Friccion? Friccion { get; set; }
    }
}
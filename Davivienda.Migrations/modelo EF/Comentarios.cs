using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("COMENTARIOS")]
    public class Comentarios
    {
        [Key]
        [Column("COM_ID")]
        public Guid COM_ID { get; set; }

        [Column("COM_COM")]
        [StringLength(255)]
        public string? COM_COM { get; set; } // El contenido del comentario

        [Column("COM_FEC_CRE")]
        public DateTimeOffset? COM_FEC_CRE { get; set; }

        [Column("COM_FEC_MOD")]
        public DateTimeOffset? COM_FEC_MOD { get; set; }

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

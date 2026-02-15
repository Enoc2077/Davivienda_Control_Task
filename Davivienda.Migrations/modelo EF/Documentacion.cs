using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("DOCUMENTACION")]
    public class Documentacion
    {
        [Key]
        [Column("DOC_ID")]
        public Guid DOC_ID { get; set; }

        [Column("DOC_NOM")]
        [StringLength(100)]
        public string? DOC_NOM { get; set; } // Nombre descriptivo del archivo

        [Column("DOC_RUT")]
        [StringLength(255)]
        public string? DOC_RUT { get; set; } // Ruta física o URL del documento

        [Column("DOC_FEC_CRE")]
        public DateTimeOffset? DOC_FEC_CRE { get; set; }

        [Column("DOC_FEC_MOD")]
        public DateTimeOffset? DOC_FEC_MOD { get; set; }

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
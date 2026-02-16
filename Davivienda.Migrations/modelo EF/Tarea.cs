using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("TAREA")]
    public class Tarea
    {
        [Key]
        [Column("TAR_ID")]
        public Guid TAR_ID { get; set; }

        [Column("TAR_NOM")]
        [StringLength(100)]
        public string? TAR_NOM { get; set; }

        [Column("TAR_DES", TypeName = "nvarchar(max)")]
        public string? TAR_DES { get; set; }

        [Column("TAR_EST")]
        [StringLength(50)]
        public string? TAR_EST { get; set; }

        [Column("TAR_FEC_INI")]
        public DateTimeOffset? TAR_FEC_INI { get; set; }

        [Column("TAR_FEC_FIN")]
        public DateTimeOffset? TAR_FEC_FIN { get; set; }

        [Column("TAR_FEC_CRE")]
        public DateTimeOffset? TAR_FEC_CRE { get; set; }

        [Column("TAR_FEC_MOD")]
        public DateTimeOffset? TAR_FEC_MOD { get; set; }

        // --- Relaciones ---

        [Column("PROC_ID")]
        public Guid? PROC_ID { get; set; }

        [ForeignKey("PROC_ID")]
        public Procesos? Proceso { get; set; }

        [Column("USU_ID")]
        public Guid? USU_ID { get; set; }

        [ForeignKey("USU_ID")]
        public Usuario? Usuario { get; set; }

        [Column("PRI_ID")]
        public Guid? PRI_ID { get; set; }

        [ForeignKey("PRI_ID")]
        public Prioridad? Prioridad { get; set; }
    }
}
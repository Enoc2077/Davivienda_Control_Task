using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("NOTIFICACIONES")]
    public class Notificaciones
    {
        [Key]
        [Column("NOT_ID")]
        public Guid NOT_ID { get; set; }

        [Column("NOT_MEN")]
        [StringLength(255)]
        public string? NOT_MEN { get; set; } // Mensaje de la notificación

        [Column("NOT_TIP")]
        [StringLength(50)]
        public string? NOT_TIP { get; set; } // Tipo (ej. Alerta, Info, Éxito)

        [Column("NOT_LEI")]
        public bool NOT_LEI { get; set; } = false; // ¿Leído? (0 o 1)

        [Column("NOT_FEC_CRE")]
        public DateTimeOffset? NOT_FEC_CRE { get; set; }

        [Column("NOT_FEC_MOD")]
        public DateTimeOffset? NOT_FEC_MOD { get; set; }

        // --- Relaciones ---

        [Column("USU_ID")]
        public Guid? USU_ID { get; set; }

        [ForeignKey("USU_ID")]
        public Usuario? Usuario { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("DETALLES_PROYECTO")]
    public class DetalleProyecto
    {
        [Key]
        [Column("DET_PRO_ID")]
        public Guid DET_PRO_ID { get; set; }

        [Required]
        [Column("PRO_ID")]
        public Guid PRO_ID { get; set; }

        [Required]
        [Column("USU_ID")]
        public Guid USU_ID { get; set; }

        [Required]
        [Column("ROL_ID")]
        public Guid ROL_ID { get; set; }

        [Column("DET_PRO_FEC_ASI")]
        public DateTimeOffset? DET_PRO_FEC_ASI { get; set; }

        [Column("DET_PRO_FEC_CRE")]
        public DateTimeOffset? DET_PRO_FEC_CRE { get; set; }

        [Column("DET_PRO_FEC_MOD")]
        public DateTimeOffset? DET_PRO_FEC_MOD { get; set; }

        // Propiedades de navegación (Relaciones)
        [ForeignKey("PRO_ID")]
        public Proyecto? Proyecto { get; set; }

        [ForeignKey("USU_ID")]
        public Usuario? Usuario { get; set; }

        [ForeignKey("ROL_ID")]
        public Roles? Rol { get; set; }
    }
}
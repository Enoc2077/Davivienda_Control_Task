using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("PRIORIDAD")]
    public class Prioridad
    {
        [Key]
        [Column("PRI_ID")]
        public Guid PRI_ID { get; set; }

        [Column("PRI_NOM")]
        [StringLength(50)]
        public string? PRI_NOM { get; set; }

        [Column("PRI_DES")]
        [StringLength(100)]
        public string? PRI_DES { get; set; }

        [Column("PRI_NIV")]
        public int? PRI_NIV { get; set; }

        [Column("PRI_FEC_CRE")]
        public DateTimeOffset? PRI_FEC_CRE { get; set; }

        [Column("PRI_FEC_MOD")]
        public DateTimeOffset? PRI_FEC_MOD { get; set; }

        // Propiedad de navegación: Lista de tareas asociadas a esta prioridad
        public virtual ICollection<Tarea>? Tareas { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("PROCESO")]
    public class Procesos
    {
        [Key]
        [Column("PROC_ID")]
        public Guid PROC_ID { get; set; }

        [Column("PROC_NOM")]
        [StringLength(100)]
        public string? PROC_NOM { get; set; }

        [Column("PROC_DES")]
        [StringLength(255)]
        public string? PROC_DES { get; set; }

        [Column("PROC_FRE")]
        [StringLength(50)]
        public string? PROC_FRE { get; set; }

        [Column("PROC_EST")]
        public bool? PROC_EST { get; set; }

        [Column("PROC_FEC_CRE")]
        public DateTimeOffset? PROC_FEC_CRE { get; set; }

        [Column("PROC_FEC_MOD")]
        public DateTimeOffset? PROC_FEC_MOD { get; set; }

        // Relación con Proyecto
        [Column("PRO_ID")]
        public Guid? PRO_ID { get; set; }

        [ForeignKey("PRO_ID")]
        public Proyecto? Proyecto { get; set; }

        // Propiedad opcional: Una lista de tareas que pertenecen a este proceso
        public virtual ICollection<Tarea>? Tareas { get; set; }
    }
}
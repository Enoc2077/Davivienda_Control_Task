using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("PROYECTO")]
    public class Proyecto
    {
        [Key]
        [Column("PRO_ID")]
        public Guid PRO_ID { get; set; }

        [Column("PRO_NOM")]
        [StringLength(100)]
        public string? PRO_NOM { get; set; }

        [Column("PRO_DES")]
        [StringLength(255)]
        public string? PRO_DES { get; set; }

        [Column("PRO_FEC_INI")]
        public DateTimeOffset? PRO_FEC_INI { get; set; }

        [Column("PRO_FEC_FIN")]
        public DateTimeOffset? PRO_FEC_FIN { get; set; }

        [Column("PRO_EST")]
        [StringLength(50)]
        public string? PRO_EST { get; set; }

        [Column("ARE_ID")]
        public Guid? ARE_ID { get; set; }

        [ForeignKey("ARE_ID")]
        public Area? Area { get; set; }

        [Column("PRO_FEC_CRE")]
        public DateTimeOffset? PRO_FEC_CRE { get; set; }

        [Column("PRO_FEC_MOD")]
        public DateTimeOffset? PRO_FEC_MOD { get; set; }
    }
}
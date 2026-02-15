using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Davivienda.Migrations.ModelosEF
{
    [Table("AREAS")] 
    public class Area
    {
        [Key]
        [Column("ARE_ID")] 
        public Guid ARE_ID { get; set; }

        [Required]
        [Column("ARE_NOM")]
        [StringLength(100)]
        public string ARE_NOM { get; set; } = string.Empty;

        [Column("ARE_DES")]
        public string? ARE_DES { get; set; }

        [Required]
        [Column("ARE_EST")]
        public bool ARE_EST { get; set; }

        [Required]
        [Column("ARE_FEC_CRE", TypeName = "datetimeoffset")]
        public DateTimeOffset ARE_FEC_CRE { get; set; }
        [Required]
        [Column("ARE_FEC_MOD", TypeName = "datetimeoffset")]
        public DateTimeOffset ARE_FEC_MOD { get; set; }
    }
}
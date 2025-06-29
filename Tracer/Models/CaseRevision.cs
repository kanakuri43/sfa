using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Models
{
    [Table("case_revisions")]
    public class CaseRevision
    {
        public int Id { get; set; }
        [Column("detected_date")]
        public DateTime DetectedDate { get; set; }
        [Column("case_id")]
        public int CaseId { get; set; }
        [Column("order_year_month")]
        public int OrderYearMonth { get; set; }
        [Column("progress_level")]
        public int ProgressLevel { get; set; }
        [Column("sale")]
        public decimal Sale { get; set; }
        [Column("profit")]
        public decimal Profit { get; set; }


    }
}

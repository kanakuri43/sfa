using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split.Models
{
    [Table("S進捗目標")]

    public class Target
    {
        [Column("月度")]
        public int YearMonth { get; set; }
        [Column("部門コード")]
        public int SectionCode { get; set; }
        [Column("社員コード")]
        public int EmployeeCode { get; set; }
        [Column("売上実績")]
        public decimal SalesTarget { get; set; }
        [Column("粗利実績")]
        public decimal ProfitTarget { get; set; }
    }
}

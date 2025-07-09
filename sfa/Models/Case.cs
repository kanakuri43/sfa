using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sfa.Models
{

    public class Case
    {
        [Column("連番")]
        public int Id { get; set; }
        [Column("削除区分")]
        public byte State { get; set; }
        [Column("名称")]
        public string CaseName { get; set; }

        [Column("受注月度")]
        public int OrderYearMonth { get; set; }
        [Column("売上月度")]
        public int SalesYearMonth { get; set; }

        [Column("売上金額")]
        public decimal SalesPrice { get; set; }
        [Column("粗利金額")]
        public decimal ProfitPrice { get; set; }
    }
}

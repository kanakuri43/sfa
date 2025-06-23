using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerMaintenance.Models
{
    [Table("M顧客ランク")]

    public class Rank
    {
        [Column("コード")]
        public byte Id { get; set; }
        [Column("削除区分")]
        public byte State { get; set; }
        [Column("名称")]
        public string Name { get; set; }
        [Column("記号")]
        public string Symbol { get; set; }
    }
}

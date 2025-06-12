using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerMaintenance.Models
{
    [Table("D顧客")]

    public class Customer
    {
        [Column("連番")]
        public int Id { get; set; }

        [Column("名称")]
        public string Name { get; set; }

        [Column("郵便番号")]
        public int ZipCode { get; set; }

        [Column("住所1")]
        public string Address1 { get; set; }

        [Column("住所2")]
        public string Address2 { get; set; }

        [Column("削除区分")]
        public byte State { get; set; }
    }
}

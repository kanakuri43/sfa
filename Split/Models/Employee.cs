using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split.Models
{
    [Table("M社員")]

    public class Employee
    {
        [Column("コード")]
        public Int16 Code { get; set; }

        [Column("部門コード")]
        public int SectionCode { get; set; }

        [Column("氏名")]
        public string Name { get; set; }

        [Column("削除区分")]
        public byte State { get; set; }
    }
}

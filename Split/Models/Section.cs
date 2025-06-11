using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split.Models
{
    [Table("M部門")]

    public class Section
    {
        [Column("コード")]
        public int Code { get; set; }
        [Column("名称")]
        public string Name { get; set; }
        [Column("削除区分")]
        public byte State { get; set; }
    }
}

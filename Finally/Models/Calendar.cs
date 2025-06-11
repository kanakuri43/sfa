using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finally.Models
{
    [Table("Mカレンダ")]
    public class Calendar
    {
        [Column("日付")]
        public int Date { get; set; }
        [Column("期")]
        public Int16 Period { get; set; }

    }
}

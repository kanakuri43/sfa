using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerMaintenance.Models
{
    public class SalesHistory
    {
        public Int16 Year { get; set; }
        public int Sales { get; set; }
        public int Profit { get; set; }
    }
}

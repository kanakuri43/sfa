using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finally.Models
{
    public class ExtendedCaseInfo : sfa.Models.Case
    {
        public Int16 ChargeEmployeeCode { get; set; }
        public string ChargeEmployeeName { get; set; }

        public int CustomerCode { get; set; }

        public string CustomerName { get; set; }
        public string Symbol { get; set; }
        public byte ProgressLevel { get; set; }

    }
}

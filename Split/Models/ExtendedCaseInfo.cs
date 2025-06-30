using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split.Models
{
    public class ExtendedCaseInfo : sfa.Models.Case
    {
        public Int16 ChargeEmployeeCode { get; set; }

        public int CustomerCode { get; set; }

        public string CustomerName { get; set; }

    }
}

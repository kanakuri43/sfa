using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseManager.Models
{
    public class ExtendedCaseInfo : sfa.Models.Case
    {
        public int ChargeEmployeeCode { get; set; }

        public int CustomerCode { get; set; }

        public string CustomerName { get; set; }
        public string Symbol { get; set; }
        public byte ProgressLevel { get; set; }

    }
}

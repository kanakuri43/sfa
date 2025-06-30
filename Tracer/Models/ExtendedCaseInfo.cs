using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Models
{
    public class ExtendedCaseInfo : sfa.Models.Case
    {
        public int RevisionCount { get; set; }
        public int EalpsedDays { get; set; }

        public int CustomerCode { get; set; }

        public string CustomerName { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Models
{
    public class ExtendedCaseRevision : CaseRevision
    {
        public string Symbol { get; set; }
        public byte ProgressLevel { get; set; }

    }
}

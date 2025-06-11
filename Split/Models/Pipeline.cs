using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Split.Models
{
    public class Pipeline
    {
        public byte Level { get; set; }
        public string Name { get; set; }
        public int CaseCount { get; set; }
        public int TotalCaseCount { get; set; }

        public double Percentage => TotalCaseCount > 0 ? (double)CaseCount / TotalCaseCount * 100 : 0.0;
    }
}

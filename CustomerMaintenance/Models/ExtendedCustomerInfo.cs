using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerMaintenance.Models
{    
    public class ExtendedCustomerInfo : Customer
    {
        public int PrimaryChargeEmployeeCode { get; set; }

        public int PrimaryChargeSectionCode { get; set; }

        public string PrimaryChargeEmployeeName { get; set; }
        public string SecondaryChargeEmployeeName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split.Models
{
    public class LatestTotal
    {
        public Int32 EmployeeCode { get; set; }
        public decimal FinishedSales { get; set; }
        public decimal FinishedProfit { get; set; }
        public decimal UnfinishedSales { get; set; }
        public decimal UnfinishedProfit { get; set; }

        public decimal TotalSales
        {
            get { return FinishedSales + UnfinishedSales; }
        }
        public decimal TotalProfit
        {
            get { return FinishedProfit + UnfinishedProfit; }
        }
        public decimal GrossMarginRate
        {
            get { return (TotalProfit / TotalSales) * 100; }
        }

    }
}

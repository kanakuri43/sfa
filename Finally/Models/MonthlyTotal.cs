using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finally.Models
{
    public class MonthlyTotal
    {
        public int YearMonth { get; set; }
        public Int32 EmployeeCode { get; set; }
        public decimal TargetSales { get; set; }
        public decimal TargetProfit { get; set; }
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
        public decimal SalesProgressRate
        {
            get { return ((FinishedSales + UnfinishedSales) / TargetSales) * 100; }
        }
        public decimal ProfitProgressRate
        {
            get { return ((FinishedProfit + UnfinishedProfit) / TargetProfit) * 100; }
        }
        public decimal GrossMarginRate
        {
            get { return TotalSales == 0 ? 0 : (TotalProfit / TotalSales) * 100; }
        }
        public int MiscIncome { get; set; }


    }
}

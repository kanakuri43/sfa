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
        public string HasUnfinishedSales { get; set; }
        public string HasUnfinishedProfit { get; set; }

        // 経費
        public decimal Cost { get; set; }
        // 営業外収益
        public decimal NonOperationProfit { get; set; }



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


        // 営業利益
        public decimal OperatingProfit
        {
            get { return TotalProfit - Cost; }
        }

        // 経常利益
        public decimal OrdinaryProfit
        {
            get { return OperatingProfit + NonOperationProfit; }
        }

        // 売上不足額
        public decimal SalesShortfall
        {
            get { return Math.Max(0, TargetSales - (FinishedSales + UnfinishedSales)); }
        }

        // 粗利不足額
        public decimal ProfitShortfall
        {
            get { return Math.Max(0, TargetProfit - (FinishedProfit + UnfinishedProfit)); }
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Split.Models
{
    [Table("S進捗")]
    public class WeeklyProgress
    {
        [Column("月度")]
        public int YearMonth { get; set; }

        [Column("日付")]
        public int Date { get; set; }

        [Column("社員コード")]
        public int EmployeeCode { get; set; }


        [Column("売上実績")]
        public decimal SalesOfRecorded { get; set; }
        [Column("売上確定")]
        public decimal SalesOfReceivedOrders { get; set; }
        [Column("売上見込1")]
        public decimal SalesOfClosing { get; set; }
        [Column("売上見込2")]
        public decimal SalesOfWish { get; set; }
        
        // 推定
        public decimal SalesOfFinalForecast 
        {
            get { return SalesOfRecorded + SalesOfReceivedOrders + SalesOfClosing + SalesOfWish; } 
        }



        [Column("粗利実績")]
        public decimal ProfitOfRecorded { get; set; }
        [Column("粗利確定")]
        public decimal ProfitOfReceivedOrders { get; set; }
        [Column("粗利見込1")]
        public decimal ProfitOfClosing { get; set; }
        [Column("粗利見込2")]
        public decimal ProfitOfWish { get; set; }

        // 推定
        public decimal ProfitOfFinalForecast
        {
            get { return ProfitOfRecorded + ProfitOfReceivedOrders + ProfitOfClosing + ProfitOfWish; }
        }

        [Column("進捗区分")]
        public byte ProgressType { get; set; }

    }
}

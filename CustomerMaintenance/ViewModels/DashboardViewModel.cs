using CustomerMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using OxyPlot;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using sfa.Models;
using Split.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CustomerMaintenance.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Customer> _customers;
        private Section _selectedSection;
        private Employee _selectedEmployee;
        private Customer _selectedCustomer;
        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<Case> _cases;
        private ObservableCollection<SalesHistory> _salesHistories;
        private OxyPlot.PlotModel _plotModel;

        public Section SelectedSection
        {
            get { return _selectedSection; }
            set { SetProperty(ref _selectedSection, value); }
        }

        public Employee SelectedEmployee
        {
            get { return _selectedEmployee; }
            set { SetProperty(ref _selectedEmployee, value); }
        }
        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }
            set { SetProperty(ref _selectedCustomer, value); }
        }

        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }
        public ObservableCollection<Section> Sections
        {
            get { return _sections; }
            set { SetProperty(ref _sections, value); }
        }
        public ObservableCollection<Employee> Employees
        {
            get { return _employees; }
            set { SetProperty(ref _employees, value); }
        }
        public ObservableCollection<Case> Cases
        {
            get { return _cases; }
            set { SetProperty(ref _cases, value); }
        }
        public ObservableCollection<SalesHistory> SalesHistories
        {
            get { return _salesHistories; }
            set { SetProperty(ref _salesHistories, value); }
        }
        public OxyPlot.PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                SetProperty(ref _plotModel, value); // BindableBaseのSetPropertyを使用
            }
        }
        public DelegateCommand SectionSelectionChanged { get; }
        public DelegateCommand EmployeeSelectionChanged { get; }
        public DelegateCommand CustomerSelectionChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand(EmployeeSelectionChangedExecute);
            CustomerSelectionChanged = new DelegateCommand(CustomerSelectionChangedExecute);


            using (var context = new AppDbContext())
            {
                Sections = new ObservableCollection<Section>(
                    context.Sections.Where(s => s.State == 0).ToList()
                );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 11010);

            }

            FetchEmployeeList();
            PlotChart();
        }


        private void PlotChart()
        {
            // null安全な実装
            if (this.SalesHistories == null || !this.SalesHistories.Any())
            {
                System.Diagnostics.Debug.WriteLine("SalesHistories is null or empty");
                PlotModel = new OxyPlot.PlotModel { Title = "データなし" };
                return;
            }
            try
            {
                var newPlotModel = new OxyPlot.PlotModel();

                // X軸（カテゴリ軸）
                var xAxis = new OxyPlot.Axes.CategoryAxis
                {
                    Position = OxyPlot.Axes.AxisPosition.Bottom,
                    ItemsSource = this.SalesHistories.Select(s => s.Year.ToString()).ToList()
                };
                newPlotModel.Axes.Add(xAxis);

                // Y軸
                var yAxis = new OxyPlot.Axes.LinearAxis()
                {
                    Position = OxyPlot.Axes.AxisPosition.Left,
                    StringFormat = "N0"
                };
                newPlotModel.Axes.Add(yAxis);

                // MahApps.Metroのテーマカラーを取得
                var accentBrush = Application.Current.Resources["MahApps.Brushes.Accent"] as SolidColorBrush;
                var accent2Brush = Application.Current.Resources["MahApps.Brushes.Accent2"] as SolidColorBrush;

                // OxyColorに変換
                var accentColor = accentBrush != null ?
                    OxyColor.FromArgb(accentBrush.Color.A, accentBrush.Color.R, accentBrush.Color.G, accentBrush.Color.B) :
                    OxyColors.Blue;
                var accent2Color = accent2Brush != null ?
                    OxyColor.FromArgb(accent2Brush.Color.A, accent2Brush.Color.R, accent2Brush.Color.G, accent2Brush.Color.B) :
                    OxyColors.Red;

                // 売上の棒グラフシリーズ
                var salesSeries = new OxyPlot.Series.ColumnSeries()
                {
                    Title = "売上",
                    ItemsSource = this.SalesHistories,
                    ValueField = "Sales",
                    FillColor = accentColor,
                    StrokeColor = OxyColors.White,
                    StrokeThickness = 1
                };
                newPlotModel.Series.Add(salesSeries);

                // 利益の棒グラフシリーズ
                var profitSeries = new OxyPlot.Series.ColumnSeries()
                {
                    Title = "利益",
                    ItemsSource = this.SalesHistories,
                    ValueField = "Profit",
                    FillColor = accent2Color,
                    StrokeColor = OxyColors.White,
                    StrokeThickness = 1
                };
                newPlotModel.Series.Add(profitSeries);

                // 凡例を表示
                newPlotModel.LegendPosition = OxyPlot.LegendPosition.TopRight;

                // プロパティに代入
                PlotModel = newPlotModel;
                PlotModel.InvalidatePlot(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PlotChart Error: {ex.Message}");
                PlotModel = new OxyPlot.PlotModel { Title = "エラーが発生しました" };
            }
        }
        private void FetchCustomerList()
        {
            // 社員未選択なら即return
            if (this.SelectedEmployee == null)
            {
                return;
            }

            using (var context = new AppDbContext())
            {
                var sql = @"
                    SELECT
                        C.連番 
                        , ISNULL(C.名称, '') AS 名称
                        , ISNULL(C.郵便番号, '') AS 郵便番号                         
                        , ISNULL(C.住所1, '') AS 住所1
                        , ISNULL(C.住所2, '') AS 住所2                        
                        , ISNULL(C.TEL, '') AS Tel
                        , ISNULL(C.削除区分, '') AS 削除区分
                    FROM
                        D顧客 AS C 
                        INNER JOIN D顧客担当 AS CS 
                            ON C.連番 = CS.顧客連番 
                            AND CS.社員コード = {0} 
                    WHERE
                        削除区分 = 0
                        ";
                var c = context.Database.SqlQueryRaw<Customer>(
                                    sql,
                                    this.SelectedEmployee.Code
                                ).ToList();
                if (c == null)
                {
                    return;
                }
                else
                {
                    this.Customers = new ObservableCollection<Customer>(c.OrderByDescending(c => c.Id));
                    ;
                }
            }
        }
        private void FetchEmployeeList()
        {

            using (var context = new AppDbContext())
            {
                Employees = new ObservableCollection<Employee>(
                    context.Employees
                        .Where(e => e.SectionCode == this.SelectedSection.Code && e.State == 0)
                        .ToList()
                );
            }
        }
        private void FetchCaseList()
        {
            if (this.SelectedCustomer == null || this.SelectedEmployee == null)
            {
                this.Cases = new ObservableCollection<Case>();
                return;
            }

            using (var context = new AppDbContext())
            {
                var sql = @"
                    SELECT
                        C.*
                        , 0 AS CustomerCode
                        , '' AS CustomerName
                        , 記号
                        , 物件確度区分                    
                    FROM
                        D物件 AS C 
                        INNER JOIN D物件顧客 AS CC 
                            ON C.連番 = CC.物件連番 
                            AND CC.顧客連番 = {0} 
                        INNER JOIN D物件担当 AS CS 
                            ON C.連番 = CS.物件連番 
                            AND CS.社員コード = {1} 
                        LEFT JOIN M物件確度 
                            ON C.物件確度 = M物件確度.コード 
                    WHERE
                        C.削除区分 = 0
                        ";
                var c = context.Database.SqlQueryRaw<Case>(
                                    sql,
                                    this.SelectedCustomer.Id,
                                    this.SelectedEmployee.Code
                                ).ToList();
                if (c == null)
                {
                    this.Cases = new ObservableCollection<Case>();
                }
                else
                {
                    this.Cases = new ObservableCollection<Case>(c.OrderByDescending(c => c.Id));
                }
            }
        }

        private void FetchSalesHistory()
        {
            if (this.SelectedCustomer == null)
            {
                this.SalesHistories = new ObservableCollection<SalesHistory>();
                return;
            }

            using (var context = new AppDbContext())
            {
                var sql = @"
                    SELECT
                        CAL.西暦 AS Year
                        , ISNULL(CONVERT(INT, (S.sales / 1000)), 0) AS sales 
                        , ISNULL(CONVERT(INT, (S.profit / 1000)), 0) AS profit 
                    FROM
                        ( 
                            SELECT
                                西暦 
                            FROM
                                Mカレンダ 
                            GROUP BY
                                西暦 
                            HAVING
                                西暦 BETWEEN YEAR(GETDATE()) - 10 AND YEAR(GETDATE())
                        ) AS CAL 
                        LEFT JOIN ( 
                            SELECT
                                受注月度 / 100 AS year
                                , SUM(D物件.売上金額) sales 
                                , SUM(D物件.粗利金額) profit 
                            FROM
                                D物件 
                                INNER JOIN D物件顧客 CC 
                                    ON 連番 = CC.物件連番 
                                INNER JOIN D顧客 CUS 
                                    ON CC.顧客連番 = CUS.連番 
                                    AND CUS.連番 = {0} 
                            WHERE
                                D物件.削除区分 = 0 
                            GROUP BY
                                受注月度 / 100
                        ) AS S
                    ON CAL.西暦 = S.year
                    ORDER BY CAL.西暦                        
                    ";
                var sh = context.Database.SqlQueryRaw<SalesHistory>(
                                    sql,
                                    this.SelectedCustomer.Id
                                ).ToList();
                if (sh == null)
                {
                    this.SalesHistories = new ObservableCollection<SalesHistory>();
                }
                else
                {
                    this.SalesHistories = new ObservableCollection<SalesHistory>(sh);
                }
            }
        }
        private void SectionSelectionChangedExecute()
        {
            FetchEmployeeList();
        }
        private void EmployeeSelectionChangedExecute()
        {
            FetchCustomerList();
        }
        private void CustomerSelectionChangedExecute()
        {
            FetchCaseList();
            FetchSalesHistory();
            PlotChart();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }
    }
}

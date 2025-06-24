using CustomerMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using OxyPlot;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using sfa.Models;
using System;
using System.Collections;
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
        private ObservableCollection<ExtendedCustomerInfo> _extendedCustomerInfos;
        private Section _selectedSection;
        private ObservableCollection<Employee> _selectedEmployees; // 複数選択用
        private Customer _selectedCustomer;
        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<Case> _cases;
        private ObservableCollection<Rank> _rankss;
        private ObservableCollection<SalesHistory> _salesHistories;
        private OxyPlot.PlotModel _plotModel;

        public Section SelectedSection
        {
            get { return _selectedSection; }
            set { SetProperty(ref _selectedSection, value); }
        }

        // 複数選択された社員のコレクション
        public ObservableCollection<Employee> SelectedEmployees
        {
            get { return _selectedEmployees; }
            set { SetProperty(ref _selectedEmployees, value); }
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
        public ObservableCollection<ExtendedCustomerInfo> ExtendedCustomerInfos
        {
            get { return _extendedCustomerInfos; }
            set { SetProperty(ref _extendedCustomerInfos, value); }
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
        public ObservableCollection<Rank> Ranks
        {
            get { return _rankss; }
            set { SetProperty(ref _rankss, value); }
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
        public DelegateCommand RankSelectionChanged { get; }
        public DelegateCommand<IList> EmployeeSelectionChanged { get; } // 複数選択対応
        public DelegateCommand CustomerSelectionChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            // 複数選択された社員のコレクションを初期化
            SelectedEmployees = new ObservableCollection<Employee>();

            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            RankSelectionChanged = new DelegateCommand(RankSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand<IList>(EmployeeSelectionChangedExecute); // 複数選択対応
            CustomerSelectionChanged = new DelegateCommand(CustomerSelectionChangedExecute);

            using (var context = new AppDbContext())
            {
                Sections = new ObservableCollection<Section>(
                    context.Sections.Where(s => s.State == 0).ToList()
                );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 11010);

                Ranks = new ObservableCollection<Rank>(
                    context.Ranks.Where(r => r.State == 0).ToList()
                );
            }

            FetchEmployeeList();
            PlotChart();
        }

        private void PlotChart()
        {
            if (this.SalesHistories == null || !this.SalesHistories.Any())
            {
                return;
            }

            try
            {
                var newPlotModel = new OxyPlot.PlotModel();

                // カテゴリ軸（X軸）を設定 - 年度
                var categoryAxis = new OxyPlot.Axes.CategoryAxis
                {
                    Position = OxyPlot.Axes.AxisPosition.Bottom,
                    ItemsSource = this.SalesHistories.Select(s => s.Year.ToString()).ToList()
                };
                newPlotModel.Axes.Add(categoryAxis);

                // 値軸（Y軸）を設定 - 売上・利益
                var yAxis = new OxyPlot.Axes.LinearAxis()
                {
                    Position = OxyPlot.Axes.AxisPosition.Left,
                    StringFormat = "N0",
                    Minimum = 0
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

                // 売上の縦棒グラフシリーズ
                var salesSeries = new OxyPlot.Series.RectangleBarSeries()
                {
                    Title = "売上",
                    FillColor = accent2Color,
                    StrokeColor = accent2Color,
                    StrokeThickness = 1
                };

                // 利益の縦棒グラフシリーズ
                var profitSeries = new OxyPlot.Series.RectangleBarSeries()
                {
                    Title = "利益",
                    FillColor = accentColor,
                    StrokeColor = accentColor,
                    StrokeThickness = 1
                };

                // データポイントを追加
                double barWidth = 0.35;
                for (int i = 0; i < this.SalesHistories.Count; i++)
                {
                    var history = this.SalesHistories[i];

                    // 売上の棒
                    salesSeries.Items.Add(new OxyPlot.Series.RectangleBarItem(
                        i - barWidth / 2, 0, i + barWidth / 2, Convert.ToDouble(history.Sales)));

                    // 利益の棒（右側に配置）
                    profitSeries.Items.Add(new OxyPlot.Series.RectangleBarItem(
                        i + barWidth / 2, 0, i + barWidth / 2 + barWidth, Convert.ToDouble(history.Profit)));
                }

                newPlotModel.Series.Add(salesSeries);
                newPlotModel.Series.Add(profitSeries);

                // 凡例を表示
                newPlotModel.IsLegendVisible = true;

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
            // 社員が選択されていない場合は即return
            if (this.SelectedEmployees == null || !this.SelectedEmployees.Any())
            {
                this.ExtendedCustomerInfos = new ObservableCollection<ExtendedCustomerInfo>();
                return;
            }

            using (var context = new AppDbContext())
            {
                var employeeCodes = this.SelectedEmployees.Select(e => e.Code).ToList();

                // IN句を使用して複数の社員コードで検索
                var sql = @"
                    SELECT
                        C.連番
                        , ISNULL(C.名称, '') AS 名称
                        , ISNULL(C.郵便番号, '') AS 郵便番号
                        , ISNULL(C.住所1, '') AS 住所1
                        , ISNULL(C.住所2, '') AS 住所2
                        , ISNULL(C.TEL, '') AS Tel
                        , ISNULL(C.FAX, '') AS Fax
                        , ISNULL(C.顧客ランク, '') AS 顧客ランク
                        , ISNULL(C.顧客地区, '') AS 地区
                        , ISNULL(C.顧客業種, '') AS 業種
                        , ISNULL(C.削除区分, '') AS 削除区分
                        , 0 AS PrimaryChargeSectionCode                        
                        , ISNULL(P.社員コード, '') AS PrimaryChargeEmployeeCode
                        , ISNULL(MP.氏名, '') AS PrimaryChargeEmployeeName
                        , ISNULL(S.社員コード, '') AS SecondaryChargeEmployeeCode
                        , ISNULL(MS.氏名, '') AS SecondaryChargeEmployeeName 
                    FROM
                        D顧客 AS C 
                        LEFT JOIN D顧客担当 AS P 
                            ON C.連番 = P.顧客連番 
                            AND P.担当区分 = 1 
                        LEFT JOIN M社員 AS MP 
                            ON P.社員コード = MP.コード 
                        LEFT JOIN D顧客担当 AS S 
                            ON C.連番 = S.顧客連番 
                            AND S.担当区分 = 2 
                        LEFT JOIN M社員 AS MS 
                            ON S.社員コード = MS.コード 
                    WHERE
                        P.社員コード IN ({0}) 
                        OR S.社員コード IN ({0})
                        ";

                // パラメータを準備
                var parameters = new object[employeeCodes.Count * 2];
                var placeholders = new List<string>();

                for (int i = 0; i < employeeCodes.Count; i++)
                {
                    placeholders.Add($"{{{i}}}");
                    parameters[i] = employeeCodes[i];
                }

                // IN句のプレースホルダーを作成
                var inClause = string.Join(",", placeholders);
                sql = sql.Replace("{0}", inClause);

                var c = context.Database.SqlQueryRaw<ExtendedCustomerInfo>(sql, parameters.Take(employeeCodes.Count).ToArray()).ToList();

                if (c == null)
                {
                    this.ExtendedCustomerInfos = new ObservableCollection<ExtendedCustomerInfo>();
                }
                else
                {
                    this.ExtendedCustomerInfos = new ObservableCollection<ExtendedCustomerInfo>(c.OrderByDescending(c => c.Id));
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
            if (this.SelectedCustomer == null || this.SelectedEmployees == null || !this.SelectedEmployees.Any())
            {
                this.Cases = new ObservableCollection<Case>();
                return;
            }

            using (var context = new AppDbContext())
            {
                var employeeCodes = this.SelectedEmployees.Select(e => e.Code).ToList();

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
                        LEFT JOIN M物件確度 
                            ON C.物件確度 = M物件確度.コード 
                    WHERE
                        C.削除区分 = 0
                        AND CS.社員コード IN ({1})
                        ";

                // パラメータを準備
                var parameters = new List<object> { this.SelectedCustomer.Id };
                var placeholders = new List<string>();

                for (int i = 0; i < employeeCodes.Count; i++)
                {
                    placeholders.Add($"{{{i + 1}}}");
                    parameters.Add(employeeCodes[i]);
                }

                // IN句のプレースホルダーを作成
                var inClause = string.Join(",", placeholders);
                sql = sql.Replace("{1}", inClause);

                var c = context.Database.SqlQueryRaw<Case>(sql, parameters.ToArray()).ToList();

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
                                西暦 BETWEEN YEAR(GETDATE()) - 10 AND YEAR(GETDATE()) - 1
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
            // 部署が変更されたら社員選択をクリア
            SelectedEmployees.Clear();
            ExtendedCustomerInfos = new ObservableCollection<ExtendedCustomerInfo>();
        }

        private void RankSelectionChangedExecute()
        {
            FetchCustomerList();
        }

        private void EmployeeSelectionChangedExecute(IList selectedItems)
        {
            // 選択された社員をコレクションに反映
            SelectedEmployees.Clear();
            if (selectedItems != null)
            {
                foreach (Employee employee in selectedItems)
                {
                    SelectedEmployees.Add(employee);
                }
            }

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
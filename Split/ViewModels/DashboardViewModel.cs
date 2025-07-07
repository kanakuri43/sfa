using ControlzEx.Standard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Split.Models;
using sfa.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Split.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private ObservableCollection<int> _years;
        private int _selectedYear;
        private ObservableCollection<int> _months;
        private int _selectedMonth;

        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<LatestTotal> _latestTotals;
        private ObservableCollection<ProgressLevel> _progressLevels;
        private ObservableCollection<ExtendedCaseInfo> _activeCases;
        private ObservableCollection<ExtendedCaseInfo> _inactiveCases;
        private ObservableCollection<Case> _customersHistories;
        private ObservableCollection<Pipeline> _pipelines;

        private Section _selectedSection;
        private Employee _selectedEmployee;
        private int _selectedProgressLevel;
        private ProgressLevel _progressLevelMin;
        private ProgressLevel _progressLevelMax;

        private decimal _currentSalesTarget;
        private float _salesProgressRate;
        private float _salesForecastProgressRate;
        private float _salesPreviousRate;

        private decimal _currentProfitTarget;
        private float _profitProgressRate;
        private float _profitForecastProgressRate;
        private float _profitPreviousRate;


        private CollectionView _resultCollectionView;


        public ObservableCollection<int> Months
        {
            get { return _months; }
            set { SetProperty(ref _months, value); }
        }
        public ObservableCollection<int> Years
        {
            get { return _years; }
            set { SetProperty(ref _years, value); }
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
        public int SelectedYear
        {
            get { return _selectedYear; }
            set { SetProperty(ref _selectedYear, value); }
        }
        public int SelectedMonth
        {
            get { return _selectedMonth; }
            set { SetProperty(ref _selectedMonth, value); }
        }

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
        public ProgressLevel ProgressLevelMin
        {
            get { return _progressLevelMin; }
            set { SetProperty(ref _progressLevelMin, value); }
        }
        public int SelectedProgressLevel
        {
            get { return _selectedProgressLevel; }
            set { SetProperty(ref _selectedProgressLevel, value); }
        }
        public ProgressLevel ProgressLevelMax
        {
            get { return _progressLevelMax; }
            set { SetProperty(ref _progressLevelMax, value); }
        }

        public decimal CurrentSalesTarget
        {
            get { return _currentSalesTarget; }
            set { SetProperty(ref _currentSalesTarget, value); }
        }
        public float SalesProgressRate
        {
            get { return _salesProgressRate; }
            set
            {
                SetProperty(ref _salesProgressRate, value);
                RaisePropertyChanged(nameof(IsSalesCompleted));
            }
        }
        public float SalesForecastProgressRate
        {
            get { return _salesForecastProgressRate; }
            set { SetProperty(ref _salesForecastProgressRate, value); }
        }
        public float SalesPreviousRate
        {
            get { return _salesPreviousRate; }
            set { SetProperty(ref _salesPreviousRate, value); }
        }
        public decimal CurrentProfitTarget
        {
            get { return _currentProfitTarget; }
            set { SetProperty(ref _currentProfitTarget, value); }
        }
        public float ProfitProgressRate
        {
            get { return _profitProgressRate; }
            set
            {
                SetProperty(ref _profitProgressRate, value);
                RaisePropertyChanged(nameof(IsProfitCompleted));
            }
        }
        public float ProfitForecastProgressRate
        {
            get { return _profitForecastProgressRate; }
            set { SetProperty(ref _profitForecastProgressRate, value); }
        }
        public float ProfitPreviousRate
        {
            get { return _profitPreviousRate; }
            set { SetProperty(ref _profitPreviousRate, value); }
        }
        public ObservableCollection<LatestTotal> LatestTotals
        {
            get { return _latestTotals; }
            set { SetProperty(ref _latestTotals, value); }
        }

        public ObservableCollection<ProgressLevel> ProgressLevels
        {
            get { return _progressLevels; }
            set { SetProperty(ref _progressLevels, value); }
        }
        public ObservableCollection<ExtendedCaseInfo> ActiveCases
        {
            get { return _activeCases; }
            set { SetProperty(ref _activeCases, value); }
        }
        public ObservableCollection<ExtendedCaseInfo> InactiveCases
        {
            get { return _inactiveCases; }
            set { SetProperty(ref _inactiveCases, value); }
        }
        public ObservableCollection<Case> CustomersHistories
        {
            get { return _customersHistories; }
            set { SetProperty(ref _customersHistories, value); }
        }

        public ObservableCollection<Pipeline> Pipelines
        {
            get { return _pipelines; }
            set { SetProperty(ref _pipelines, value); }
        }

        public CollectionView ResultCollectionView
        {
            get { return _resultCollectionView; }
            set { SetProperty(ref _resultCollectionView, value); }
        }
        public bool IsSalesCompleted
        {
            get { return SalesProgressRate >= 100; }
        }
        public bool IsProfitCompleted
        {
            get { return ProfitProgressRate >= 100; }
        }


        public DelegateCommand YearSelectionChanged { get; }
        public DelegateCommand MonthSelectionChanged { get; }
        public DelegateCommand SectionSelectionChanged { get; }
        public DelegateCommand EmployeeSelectionChanged { get; }
        public DelegateCommand SelectedProgressLevelChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            YearSelectionChanged = new DelegateCommand(YearSelectionChangedExecute);
            MonthSelectionChanged = new DelegateCommand(MonthSelectionChangedExecute);
            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand(EmployeeSelectionChangedExecute);
            SelectedProgressLevelChanged = new DelegateCommand(SelectedProgressLevelChangedExecute);

            // 年リスト
            int currentYear = DateTime.Now.Year;
            Years = new ObservableCollection<int>(Enumerable.Range(currentYear - 1, 3));
            this.SelectedYear = currentYear;

            // 月リスト
            Months = new ObservableCollection<int>(Enumerable.Range(1, 12));
            this.SelectedMonth = DateTime.Now.Month;

            using (var context = new AppDbContext())
            {
                // 部署リスト
                Sections = new ObservableCollection<Section>(
                            context.Sections.Where(s => s.State == 0).ToList()
                        );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 21130);

                // 案件リスト
                var sql = @"
                    SELECT
                        L3.コード
                        , CASE 
                            WHEN L3.部門レベル = 3 
                                THEN L1.略称 + '／' + L2.略称 + '／' + L3.略称 
                            WHEN L3.部門レベル = 2 
                                THEN L1.略称 + '／' + L2.略称 
                            ELSE L3.略称 
                            END AS 名称 
                        , L3.削除区分
                    FROM
                        M部門 L3 
                        LEFT JOIN M部門 L1 
                            ON L3.事業部コード = L1.事業部コード 
                            AND L3.コード - L3.コード % 10000 = L1.コード 
                        LEFT JOIN M部門 L2 
                            ON L3.事業部コード = L2.事業部コード 
                            AND L3.コード - L3.コード % 100 = L2.コード 
                    WHERE
                        L3.削除区分 = 0
                    ORDER BY
                        L3.コード
                        ";
                var s = context.Database.SqlQueryRaw<Section>(
                                    sql
                                ).ToList();
                if (s == null)
                {
                    return;
                }
                else
                {
                    this.Sections = new ObservableCollection<Section>(s);
                    ;
                }


                // 物権確度
                this.ProgressLevels = new ObservableCollection<ProgressLevel>(
                                context.ProgressLevels.Where(s => s.State == 0).ToList()
                            );

                var sortedProgressLevels = ProgressLevels
                    .Where(pl => pl.State == 0 && pl.Level <= 20 && pl.Level >= 1)
                    .OrderByDescending(pl => pl.Level)
                    .ToList();
                ProgressLevelMax = sortedProgressLevels[0];
                SelectedProgressLevel = 4;
                ProgressLevelMin = sortedProgressLevels[SelectedProgressLevel];

            }

            // 社員リスト 部署変更時に再度呼び出すので関数化
            FetchEmployeeList();

            UpdateScreen();
        }

        private void UpdateScreen()
        {
            // 社員未選択なら即return
            if (this.SelectedEmployee == null)
            {
                ResultCollectionView = new ListCollectionView(new ObservableCollection<WeeklyProgress>());
                return;
            }

            using (var context = new AppDbContext())
            {
                // 選択月の目標金額
                var target = context.Set<Target>()
                                    .FirstOrDefault(t => t.EmployeeCode == SelectedEmployee.Code && t.YearMonth == (SelectedYear * 100 + SelectedMonth));
                if (target != null)
                {
                    this.CurrentSalesTarget = target.SalesTarget;
                    this.CurrentProfitTarget = target.ProfitTarget;
                }


                // 選択月の 売上・粗利
                var weeklyProgress = context.Set<WeeklyProgress>()
                    .Where(wp => wp.EmployeeCode == this.SelectedEmployee.Code && wp.YearMonth == (this.SelectedYear * 100 + this.SelectedMonth))
                    .Select(wp => new WeeklyProgress
                    {
                        Date = (wp.Date % 100),
                        SalesOfRecorded = wp.SalesOfRecorded,   
                        SalesOfClosing = wp.SalesOfClosing,
                        SalesOfWish = wp.SalesOfWish,
                        ProfitOfRecorded = wp.ProfitOfRecorded,
                        ProfitOfClosing = wp.ProfitOfClosing,
                        ProfitOfWish = wp.ProfitOfWish
                    })
                    .ToList();
                ResultCollectionView = new ListCollectionView(new ObservableCollection<WeeklyProgress>(weeklyProgress));


                // 社員ごとの最終金額
                var sql = @"
                        SELECT
                            F.社員コード AS EmployeeCode
                            , ISNULL(F.FinishedSales, 0) AS FinishedSales
                            , ISNULL(F.FinishedProfit, 0) AS FinishedProfit
                            , ISNULL(U.UnfinishedSales, 0) AS UnfinishedSales
                            , ISNULL(U.UnfinishedProfit, 0) AS UnfinishedProfit 
                        FROM
                            ( 
                                SELECT
                                    D物件担当.社員コード
                                    , ISNULL(SUM(D物件.売上金額), 0) AS FinishedSales
                                    , ISNULL(SUM(D物件.粗利金額), 0) AS FinishedProfit 
                                FROM
                                    D物件 
                                    INNER JOIN D物件担当 
                                        ON D物件担当.物件連番 = D物件.連番 
                                        AND D物件担当.担当区分 = 1 
                                    LEFT JOIN M物件確度 
                                        ON M物件確度.コード = D物件.物件確度 
                                WHERE
                                    D物件担当.社員コード = {0} 
                                    AND D物件.受注月度 = {1} 
                                    AND D物件.削除区分 = 0 
                                    AND M物件確度.物件確度区分 BETWEEN 30 AND 100 
                                GROUP BY
                                    D物件担当.社員コード
                            ) F 
                            LEFT JOIN ( 
                                SELECT
                                    D物件担当.社員コード
                                    , ISNULL(SUM(D物件.売上金額), 0) AS UnfinishedSales
                                    , ISNULL(SUM(D物件.粗利金額), 0) AS UnfinishedProfit 
                                FROM
                                    D物件 
                                    INNER JOIN D物件担当 
                                        ON D物件担当.物件連番 = D物件.連番 
                                        AND D物件担当.担当区分 = 1 
                                    LEFT JOIN M物件確度 
                                        ON M物件確度.コード = D物件.物件確度 
                                WHERE
                                    D物件担当.社員コード = {0}
                                    AND D物件.受注月度 = {1} 
                                    AND D物件.削除区分 = 0 
                                    AND M物件確度.物件確度区分 >= {2}
                                    AND M物件確度.物件確度区分 <= {3}
                                GROUP BY
                                    D物件担当.社員コード
                            ) U 
                                ON F.社員コード = U.社員コード";
                var la = context.Database.SqlQueryRaw<LatestTotal>(
                                    sql,
                                    this.SelectedEmployee.Code,
                                    this.SelectedYear * 100 + this.SelectedMonth,
                                    this.ProgressLevelMin.Level,
                                    this.ProgressLevelMax.Level
                                ).FirstOrDefault();
                if (la == null)
                {
                    this.LatestTotals = new ObservableCollection<LatestTotal>();
                    SalesProgressRate = 0;
                    SalesForecastProgressRate = 0;
                    ProfitProgressRate = 0;
                    ProfitForecastProgressRate = 0;

                }
                else
                { 
                    this.LatestTotals = new ObservableCollection<LatestTotal> { la };

                    // 達成率
                    if (CurrentSalesTarget > 0)
                    {
                        SalesProgressRate = ((float)(LatestTotals[0].FinishedSales / CurrentSalesTarget) * 100);
                        SalesForecastProgressRate = ((float)((LatestTotals[0].FinishedSales + LatestTotals[0].UnfinishedSales) / CurrentSalesTarget) * 100);
                    }
                    else
                    {
                        SalesProgressRate = 0;
                        SalesForecastProgressRate = 0;
                    }
                    if (CurrentProfitTarget > 0)
                    {
                        ProfitProgressRate = ((float)(LatestTotals[0].FinishedProfit / CurrentProfitTarget) * 100);
                        ProfitForecastProgressRate = ((float)((LatestTotals[0].FinishedProfit + LatestTotals[0].UnfinishedProfit) / CurrentProfitTarget) * 100);
                    }
                    else
                    {
                        ProfitProgressRate = 0;
                        ProfitForecastProgressRate = 0;
                    }
                }

                // 案件リスト
                sql = @"
                        SELECT
                            D物件.*
                            , C.連番 AS CustomerCode
                            , C.名称 AS CustomerName
                            , 記号 AS Symbol
                            , 物件確度区分 AS ProgressLevel
                            , {0} AS ChargeEmployeeCode
                        FROM
                            D物件 
                            INNER JOIN D物件担当 
                                ON D物件担当.物件連番 = D物件.連番 
                                AND D物件担当.担当区分 = 1 
                            LEFT JOIN M物件確度 
                                ON M物件確度.コード = D物件.物件確度 
							LEFT JOIN D物件顧客 CC
							    ON D物件.連番 = CC.物件連番
							LEFT JOIN D顧客 C
							    ON CC.顧客連番 = C.連番
                        WHERE
                            D物件担当.社員コード = {0} 
                            AND D物件.受注月度 = {1} 
                            AND D物件.削除区分 = 0 
                            AND M物件確度.物件確度区分 >= {2}
                            AND M物件確度.物件確度区分 <= {3}
                        ";
                var ac = context.Database.SqlQueryRaw<ExtendedCaseInfo>(
                                    sql,
                                    this.SelectedEmployee.Code,
                                    this.SelectedYear * 100 + this.SelectedMonth,
                                    this.ProgressLevelMin.Level,
                                    this.ProgressLevelMax.Level
                                ).ToList();
                if (ac == null)
                {
                    return;
                }
                else
                {
                    this.ActiveCases = new ObservableCollection<ExtendedCaseInfo>(ac.OrderByDescending(ac => ac.ProgressLevel));
                    ;
                }

                sql = @"
                        SELECT
                            D物件.*
                            , C.連番 AS CustomerCode
                            , C.名称 AS CustomerName
                            , 記号 AS Symbol
                            , 物件確度区分 AS ProgressLevel
                            , {0} AS ChargeEmployeeCode
                        FROM
                            D物件 
                            INNER JOIN D物件担当 
                                ON D物件担当.物件連番 = D物件.連番 
                                AND D物件担当.担当区分 = 1 
                            LEFT JOIN M物件確度 
                                ON M物件確度.コード = D物件.物件確度 
							LEFT JOIN D物件顧客 CC
							    ON D物件.連番 = CC.物件連番
							LEFT JOIN D顧客 C
							    ON CC.顧客連番 = C.連番
                        WHERE
                            D物件担当.社員コード = {0} 
                            AND D物件.受注月度 = {1} 
                            AND D物件.削除区分 = 0 
                            AND M物件確度.物件確度区分 >= 30
                        ";
                var ic = context.Database.SqlQueryRaw<ExtendedCaseInfo>(
                                    sql,
                                    this.SelectedEmployee.Code,
                                    this.SelectedYear * 100 + this.SelectedMonth,
                                    this.ProgressLevelMin.Level,
                                    this.ProgressLevelMax.Level
                                ).ToList();
                if (ic == null)
                {
                    return;
                }
                else
                {
                    this.InactiveCases = new ObservableCollection<ExtendedCaseInfo>(ic.OrderByDescending(ic => ic.ProgressLevel));
                    ;
                }

                // パイプライン
                sql = @"
                        WITH TotalCount AS (
                            SELECT COUNT(D物件担当.物件連番) AS TotalCaseCount
                            FROM M物件確度 
                                LEFT JOIN D物件 
                                    ON M物件確度.コード = D物件.物件確度 
                                    AND D物件.削除区分 = 0 
                                LEFT JOIN D物件担当 
                                    ON D物件担当.物件連番 = D物件.連番 
                                    AND D物件担当.担当区分 = 1 
                                    AND D物件担当.社員コード = {0} 
                            WHERE M物件確度.削除区分 = 0 
                                AND M物件確度.物件確度区分 BETWEEN 1 AND 20
                        )

                        -- メインクエリ：物件確度区分別の集計
                        SELECT
                            物件確度区分 AS Level,
                            MIN(M物件確度.記号) AS Name,
                            COUNT(D物件担当.物件連番) AS CaseCount,
                            (SELECT TotalCaseCount FROM TotalCount) AS TotalCaseCount
                        FROM M物件確度 
                            LEFT JOIN D物件 
                                ON M物件確度.コード = D物件.物件確度 
                                AND D物件.削除区分 = 0 
                            LEFT JOIN D物件担当 
                                ON D物件担当.物件連番 = D物件.連番 
                                AND D物件担当.担当区分 = 1 
                                AND D物件担当.社員コード = {0} 
                        WHERE M物件確度.削除区分 = 0 
                            AND M物件確度.物件確度区分 BETWEEN 1 AND 20 
                        GROUP BY 物件確度区分 
                        ORDER BY 物件確度区分;                        
                    ";
                var p = context.Database.SqlQueryRaw<Pipeline>(
                                    sql,
                                    this.SelectedEmployee.Code,
                                    this.SelectedYear * 100 + this.SelectedMonth,
                                    this.ProgressLevelMin.Level,
                                    this.ProgressLevelMax.Level
                                ).ToList();
                if (p == null)
                {
                    return;
                }
                else
                {
                    this.Pipelines = new ObservableCollection<Pipeline>(p.OrderBy(p => p.Level));
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

        private void YearSelectionChangedExecute()
        {
            UpdateScreen();
        }
        private void MonthSelectionChangedExecute()
        {
            UpdateScreen();
        }
        private void SectionSelectionChangedExecute()
        {
            FetchEmployeeList();
        }
        private void EmployeeSelectionChangedExecute()
        {
            UpdateScreen();
        }
        private void SelectedProgressLevelChangedExecute()
        {
            var sortedProgressLevels = ProgressLevels
                .Where(pl => pl.State == 0 && pl.Level <= 20 && pl.Level >= 1)
                .OrderByDescending(pl => pl.Level)  
                .ToList();
            // 選択されたProgressLevelを取得
            if (SelectedProgressLevel >= 0 && SelectedProgressLevel < sortedProgressLevels.Count)
            {
                ProgressLevelMin = sortedProgressLevels[SelectedProgressLevel];
            }
            else
            {
                ProgressLevelMin = null; // 範囲外の場合はnullを設定
            }

            UpdateScreen();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}

using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Finally.Models;
using Microsoft.EntityFrameworkCore;

namespace Finally.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private ObservableCollection<int> _years;
        private int _selectedYear;
        private ObservableCollection<int> _months;
        private int _selectedMonth;
        private int _period;
        private MonthlyTotal _selectedMonthlyTotal;

        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<ProgressLevel> _progressLevels;
        private ObservableCollection<Case> _cases;
        private ObservableCollection<MonthlyTotal> _monthlyTotals;
        private ObservableCollection<MonthlyTotal> _yearlyTotals;
        private ObservableCollection<Calendar> _calendars;

        private Section _selectedSection;
        private Employee _selectedEmployee;
        private int _selectedProgressLevel;
        private ProgressLevel _progressLevelMin;
        private ProgressLevel _progressLevelMax;


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
        public int Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value); }
        }
        public MonthlyTotal SelectedMonthlyTotal
        {
            get { return _selectedMonthlyTotal; }
            set { SetProperty(ref _selectedMonthlyTotal, value); }
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
        public ObservableCollection<Calendar> Calendars
        {
            get { return _calendars; }
            set { SetProperty(ref _calendars, value); }
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
        public ObservableCollection<ProgressLevel> ProgressLevels
        {
            get { return _progressLevels; }
            set { SetProperty(ref _progressLevels, value); }
        }
        public ObservableCollection<MonthlyTotal> MonthlyTotals
        {
            get { return _monthlyTotals; }
            set { SetProperty(ref _monthlyTotals, value); }
        }
        public ObservableCollection<MonthlyTotal> YearlyTotals
        {
            get { return _yearlyTotals; }
            set { SetProperty(ref _yearlyTotals, value); }
        }
        public ObservableCollection<Case> Cases
        {
            get { return _cases; }
            set { SetProperty(ref _cases, value); }
        }

        public DelegateCommand YearSelectionChanged { get; }
        public DelegateCommand MonthSelectionChanged { get; }
        public DelegateCommand SectionSelectionChanged { get; }
        public DelegateCommand EmployeeSelectionChanged { get; }
        public DelegateCommand SelectedProgressLevelChanged { get; }
        public DelegateCommand MonthlyTotalSelectionChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            YearSelectionChanged = new DelegateCommand(YearSelectionChangedExecute);
            MonthSelectionChanged = new DelegateCommand(MonthSelectionChangedExecute);
            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand(EmployeeSelectionChangedExecute);
            SelectedProgressLevelChanged = new DelegateCommand(SelectedProgressLevelChangedExecute);
            MonthlyTotalSelectionChanged = new DelegateCommand(MonthlyTotalSelectionChangedExecute);

            // 年リスト
            int currentYear = DateTime.Now.Year;
            Years = new ObservableCollection<int>(Enumerable.Range(currentYear - 1, 3));
            //this.SelectedYear = currentYear;

            // 月リスト
            Months = new ObservableCollection<int>(Enumerable.Range(1, 12));
            this.SelectedMonth = DateTime.Now.Month;


            using (var context = new AppDbContext())
            {
                // 今日の期を求める
                int todayDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                var c1 = context.Calendars.FirstOrDefault(c => c.Date == todayDate);
                if (c1 != null)
                {
                    this.Period = c1.Period; 
                }
                // 期の最初の日付の年
                var c2 = context.Calendars
                           .Where(c => c.Period == this.Period)
                           .OrderBy(c => c.Date)
                           .FirstOrDefault();
                if (c2 != null)
                {
                    DateTime date = DateTime.ParseExact(c2.Date.ToString(), "yyyyMMdd", null);
                    this.SelectedYear = date.Year;
                }


                // 部署リスト
                Sections = new ObservableCollection<Section>(
                            context.Sections.Where(s => s.State == 0).ToList()
                        );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 21130);

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

            FetchEmployeeList();

            UpdateScreen();

        }

        private void UpdateCases()
        {
            using (var context = new AppDbContext())
            {
                // 案件リスト
                if (this.SelectedMonthlyTotal != null)
                {

                    var sql = @"
                            SELECT
                                D物件.*
                                , 記号
                                , 物件確度区分
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
                            ";
                    var c = context.Database.SqlQueryRaw<Case>(
                                        sql,
                                        this.SelectedEmployee.Code,
                                        this.SelectedMonthlyTotal.YearMonth,
                                        this.ProgressLevelMin.Level,
                                        this.ProgressLevelMax.Level
                                    ).ToList();
                    if (c == null)
                    {
                        return;
                    }
                    else
                    {
                        this.Cases = new ObservableCollection<Case>(c.OrderByDescending(c => c.Level));
                    }

                }

            } 
        }
        private void UpdateScreen()
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
                                CAL.月度 AS YearMonth
                                , TAR.売上目標 AS TargetSales
                                , TAR.粗利目標 AS TargetProfit
                                , ISNULL(S.社員コード, 0) AS EmployeeCode
                                , ISNULL(S.FinishedSales, 0) AS FinishedSales
                                , ISNULL(S.FinishedProfit, 0) AS FinishedProfit 
                                , ISNULL(U.UnfinishedSales, 0) AS UnfinishedSales
                                , ISNULL(U.UnfinishedProfit, 0) AS UnfinishedProfit 
                                , 0 AS MiscIncome 
                            FROM
                                (select 月度 FROM Mカレンダ WHERE 期 = {4} GROUP BY 月度) CAL 
                                LEFT JOIN ( 
                                    SELECT
                                        月度
                                        , SUM(売上実績) as 売上目標
                                        , SUM(粗利実績) as 粗利目標 
                                    FROM
                                        S進捗目標 
                                    WHERE
                                        進捗区分 = 1 
                                        AND 社員コード <> 0 
                                        and (社員コード = {0} OR (0 = {0})) 
                                        and 部門コード between {1} and {1} 
                                    GROUP BY
                                        月度
                                ) AS TAR 
                                    ON CAL.月度 = TAR.月度 
                                LEFT JOIN ( 
                                    SELECT
                                        D物件.受注月度
                                        , D物件担当.社員コード
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
                                        AND D物件.削除区分 = 0 
                                        AND M物件確度.物件確度区分 BETWEEN 30 AND 100 
                                    GROUP BY
                                        D物件担当.社員コード
                                        , D物件.受注月度
                                ) AS S 
                                    ON CAL.月度 = S.受注月度
                                LEFT JOIN ( 
                                    SELECT
                                        D物件.受注月度
                                        , D物件担当.社員コード
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
                                        AND D物件.削除区分 = 0 
                                        AND M物件確度.物件確度区分 >= {2}
                                        AND M物件確度.物件確度区分 <= {3}
                                    GROUP BY
                                        D物件担当.社員コード
                                        , D物件.受注月度
                                ) U 
                                    ON CAL.月度 = U.受注月度

                        ";
                var lt = context.Database.SqlQueryRaw<MonthlyTotal>(
                                    sql,
                                    this.SelectedEmployee.Code,
                                    this.SelectedSection.Code,
                                    this.ProgressLevelMin.Level,
                                    this.ProgressLevelMax.Level,
                                    this.Period
                                ).ToList();
                if (lt == null)
                {
                    return;
                }
                else
                {
                    this.MonthlyTotals = new ObservableCollection<MonthlyTotal>(lt);
                    var yt = new MonthlyTotal
                    {
                        YearMonth = this.SelectedYear,
                        EmployeeCode = this.SelectedEmployee.Code,
                        TargetSales = MonthlyTotals.Sum(s => s.TargetSales),
                        TargetProfit = MonthlyTotals.Sum(s => s.TargetProfit),
                        FinishedSales = MonthlyTotals.Sum(s => s.FinishedSales),
                        FinishedProfit = MonthlyTotals.Sum(s => s.FinishedProfit),
                        UnfinishedSales = MonthlyTotals.Sum(s => s.UnfinishedSales),
                        UnfinishedProfit = MonthlyTotals.Sum(s => s.UnfinishedProfit)

                    };
                    this.YearlyTotals = new ObservableCollection<MonthlyTotal> { yt };
                }

                this.Cases = new ObservableCollection<Case>();


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
        private void MonthlyTotalSelectionChangedExecute()
        {
            UpdateCases();
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

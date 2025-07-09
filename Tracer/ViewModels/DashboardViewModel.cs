using Microsoft.EntityFrameworkCore;
using OxyPlot;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using sfa.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Tracer.Models;
using System.Windows;

namespace Tracer.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private ObservableCollection<int> _alertElapsedDays;
        private int _selectedAlertElapsedDay;
        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private Section _selectedSection;
        private Employee _selectedEmployee;
        private Case _selectedCase;
        private ObservableCollection<ProgressLevel> _progressLevels;
        private int _selectedProgressLevel;
        private ProgressLevel _progressLevelMin;
        private ProgressLevel _progressLevelMax;
        private ObservableCollection<ExtendedCaseInfo> _cases;
        private ObservableCollection<ExtendedCaseRevision> _caseRevisions;
        private ObservableCollection<Pipeline> _pipelines;
        private string _selectedSectionCode;
        private OxyPlot.PlotModel _plotModel;

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
        public Section SelectedSection
        {
            get { return _selectedSection; }
            set { SetProperty(ref _selectedSection, value); }
        }
        public Case SelectedCase
        {
            get { return _selectedCase; }
            set { SetProperty(ref _selectedCase, value); }
        }

        public Employee SelectedEmployee
        {
            get { return _selectedEmployee; }
            set { SetProperty(ref _selectedEmployee, value); }
        }
        public ObservableCollection<ProgressLevel> ProgressLevels
        {
            get { return _progressLevels; }
            set { SetProperty(ref _progressLevels, value); }
        }
        public int SelectedProgressLevel
        {
            get { return _selectedProgressLevel; }
            set { SetProperty(ref _selectedProgressLevel, value); }
        }
        public ProgressLevel ProgressLevelMin
        {
            get { return _progressLevelMin; }
            set { SetProperty(ref _progressLevelMin, value); }
        }
        public ProgressLevel ProgressLevelMax
        {
            get { return _progressLevelMax; }
            set { SetProperty(ref _progressLevelMax, value); }
        }
        public ObservableCollection<ExtendedCaseInfo> Cases
        {
            get { return _cases; }
            set { SetProperty(ref _cases, value); }
        }
        public ObservableCollection<ExtendedCaseRevision> CaseRevisions
        {
            get { return _caseRevisions; }
            set { SetProperty(ref _caseRevisions, value); }
        }
        public ObservableCollection<int> AlertElapsedDays
        {
            get { return _alertElapsedDays; }
            set { SetProperty(ref _alertElapsedDays, value); }
        }
        public int SelectedAlertElapsedDay
        {
            get { return _selectedAlertElapsedDay; }
            set { SetProperty(ref _selectedAlertElapsedDay, value); }
        }
        public ObservableCollection<Pipeline> Pipelines
        {
            get { return _pipelines; }
            set { SetProperty(ref _pipelines, value); }
        }
        public string SelectedSectionCode
        {
            get { return _selectedSectionCode; }
            set { SetProperty(ref _selectedSectionCode, value); }
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
        public DelegateCommand SelectedProgressLevelChanged { get; }
        public DelegateCommand CaseSelectionChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand(EmployeeSelectionChangedExecute);
            SelectedProgressLevelChanged = new DelegateCommand(SelectedProgressLevelChangedExecute);
            CaseSelectionChanged = new DelegateCommand(CaseSelectionChangedExecute);

            using (var context = new AppDbContext())
            {
                // 部署リスト
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
                        INNER JOIN M部門 L1 
                            ON L3.事業部コード = L1.事業部コード 
                            AND L3.コード - L3.コード % 10000 = L1.コード 
                        INNER JOIN M部門 L2 
                            ON L3.事業部コード = L2.事業部コード 
                            AND L3.コード - L3.コード % 100 = L2.コード 
                    WHERE
                        L3.削除区分 = 0
                    ORDER BY
                        L3.コード
                        ";
                var s = context.Database.SqlQueryRaw<Section>(sql).ToList();
                if (s == null)
                {
                    return;
                }
                else
                {
                    this.Sections = new ObservableCollection<Section>(s);
                }
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 21130);
                if (this.SelectedSection != null)
                {
                    this.SelectedSectionCode = this.SelectedSection.Code.ToString();
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

            AlertElapsedDays = new ObservableCollection<int>(Enumerable.Range(1, 4).Select(x => x * 7));
            SelectedAlertElapsedDay = 7;
            // 社員リスト 部署変更時に再度呼び出すので関数化
            FetchEmployeeList();

            UpdateScreen();

        }

        private void FetchEmployeeList()
        {
            using (var context = new AppDbContext())
            {
                string sectionCodeStr = this.SelectedSection.Code.ToString("D5");
                string searchPrefix = sectionCodeStr.TrimEnd('0');

                Employees = new ObservableCollection<Employee>(
                    context.Employees
                        .Where(e => e.State == 0)
                        .AsEnumerable()
                        .Where(e => e.SectionCode.ToString("D5").StartsWith(searchPrefix))
                        .ToList()
                );
            }
        }

        private void FetchCaseRevisions()
        {
            if (this.SelectedCase == null)
            {
                return;
            }

            using (var context = new AppDbContext())
            {
                var sql = $@"
                        SELECT
                            case_revisions.*
                            , 記号 AS ProgressSymbol
                            , 物件確度区分 AS ProgressLevel
                        FROM
                            case_revisions 
                            INNER JOIN M物件確度 
                                ON case_revisions.progress_level = M物件確度.コード 
                        WHERE
                            case_revisions.case_id = {this.SelectedCase.Id} 
                        ORDER BY
                            case_revisions.detected_date DESC
                        ";
                var c = context.Database.SqlQueryRaw<ExtendedCaseRevision>(sql).ToList();
                if (c == null)
                {
                    return;
                }
                else
                {
                    this.CaseRevisions = new ObservableCollection<ExtendedCaseRevision>(c);
                }
            }
        }

        private void UpdateScreen()
        {
            if (this.SelectedEmployee == null)
            {
                return;
            }

            using (var context = new AppDbContext())
            {
                // 案件リスト
                var sql = $@"
                        SELECT
                            D物件.*
                            , ISNULL(C.連番, 0) AS CustomerCode
                            , ISNULL(C.名称, 0) AS CustomerName
                            , 記号 AS ProgressSymbol
                            , 物件確度区分 AS ProgressLevel
                            , ISNULL(CR.revision_count, 0) AS RevisionCount
                            , ISNULL(CR.elapsed_days, 0) AS EalpsedDays
                            , (CASE WHEN ISNULL(CR.revision_count, 0) = 0 THEN '*' ELSE '' END) AS Sign
                        FROM
                            D物件 
                            INNER JOIN D物件担当 
                                ON D物件担当.物件連番 = D物件.連番 
                                AND D物件担当.担当区分 = 1 
                            INNER JOIN M物件確度 
                                ON M物件確度.コード = D物件.物件確度 
                                AND M物件確度.物件確度区分 >= {this.ProgressLevelMin.Level}
                                AND M物件確度.物件確度区分 <= {this.ProgressLevelMax.Level}
							INNER JOIN D物件顧客 CC
							    ON D物件.連番 = CC.物件連番
							INNER JOIN D顧客 C
							    ON CC.顧客連番 = C.連番
							INNER JOIN (
                                SELECT 
                                    case_id
                                    , count(1) AS revision_count
                                    , DATEDIFF(DAY, MAX(detected_date), GETDATE()) AS elapsed_days
                                FROM
                                    case_revisions 
                                GROUP BY
                                    case_id
                                ) AS CR
							    ON D物件.連番 = CR.case_id
                        WHERE
                            D物件担当.社員コード = {this.SelectedEmployee.Code} 
                            AND D物件.削除区分 = 0 
                        ";
                var c = context.Database.SqlQueryRaw<ExtendedCaseInfo>(sql).ToList();
                if (c == null)
                {
                    return;
                }
                else
                {
                    this.Cases = new ObservableCollection<ExtendedCaseInfo>(c.OrderByDescending(c => c.ProgressLevel));                
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
                PlotChart();
            }
        }
        private void SectionSelectionChangedExecute()
        {
            FetchEmployeeList();
        }
        private void EmployeeSelectionChangedExecute()
        {
            UpdateScreen();
        }

        private void CaseSelectionChangedExecute()
        {
            FetchCaseRevisions();
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

        private void PlotChart()
        {
            if (this.Pipelines == null || !this.Pipelines.Any())
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
                    ItemsSource = this.Pipelines.Select(p => p.Name.ToString()).ToList()
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
                for (int i = 0; i < this.Pipelines.Count; i++)
                {
                    var pl = this.Pipelines[i];

                    // 売上の棒
                    salesSeries.Items.Add(new OxyPlot.Series.RectangleBarItem(
                        i - barWidth / 2, 0, i + barWidth / 2, Convert.ToDouble(pl.CaseCount)));

                }

                newPlotModel.Series.Add(salesSeries);

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

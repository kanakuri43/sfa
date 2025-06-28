using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using sfa.Models;
using Microsoft.EntityFrameworkCore;
using Tracer.Models;
using System.Windows.Data;

namespace Tracer.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private Section _selectedSection;
        private Employee _selectedEmployee;
        private ObservableCollection<ProgressLevel> _progressLevels;
        private int _selectedProgressLevel;
        private ProgressLevel _progressLevelMin;
        private ProgressLevel _progressLevelMax;
        private ObservableCollection<Case> _cases;

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
        public ObservableCollection<Case> Cases
        {
            get { return _cases; }
            set { SetProperty(ref _cases, value); }
        }

        public DelegateCommand SectionSelectionChanged { get; }
        public DelegateCommand EmployeeSelectionChanged { get; }
        public DelegateCommand SelectedProgressLevelChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand(EmployeeSelectionChangedExecute);
            SelectedProgressLevelChanged = new DelegateCommand(SelectedProgressLevelChangedExecute);

            using (var context = new AppDbContext())
            {
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

            // 社員リスト 部署変更時に再度呼び出すので関数化
            FetchEmployeeList();

            UpdateScreen();

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
                            , C.連番 AS CustomerCode
                            , C.名称 AS CustomerName
                            , 記号
                            , 物件確度区分
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
                            D物件担当.社員コード = {this.SelectedEmployee.Code} 
                            AND D物件.削除区分 = 0 
                            AND M物件確度.物件確度区分 >= {this.ProgressLevelMin.Level}
                            AND M物件確度.物件確度区分 <= {this.ProgressLevelMax.Level}
                        ";
                var c = context.Database.SqlQueryRaw<Case>(sql).ToList();
                if (c == null)
                {
                    return;
                }
                else
                {
                    this.Cases = new ObservableCollection<Case>(c.OrderByDescending(c => c.Level));
                    ;
                }

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

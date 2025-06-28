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

using CaseManager.Models;
using Microsoft.EntityFrameworkCore;
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

namespace CaseManager.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Customer> _customers;
        private Section _selectedSection;
        private ObservableCollection<Employee> _selectedEmployees; // 複数選択用
        private Customer _selectedCustomer;
        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<Case> _cases;
        private ObservableCollection<ExtendedCaseInfo> _extendedCaseInfos;

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
        public ObservableCollection<ExtendedCaseInfo> ExtendedCaseInfos
        {
            get { return _extendedCaseInfos; }
            set { SetProperty(ref _extendedCaseInfos, value); }
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
            EmployeeSelectionChanged = new DelegateCommand<IList>(EmployeeSelectionChangedExecute); // 複数選択対応

            using (var context = new AppDbContext())
            {
                Sections = new ObservableCollection<Section>(
                    context.Sections.Where(s => s.State == 0).ToList()
                );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 11010);

            }

            FetchEmployeeList();


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
            // 社員が選択されていない場合は即return
            if (this.SelectedEmployees == null || !this.SelectedEmployees.Any())
            {
                this.ExtendedCaseInfos = new ObservableCollection<ExtendedCaseInfo>();
                return;
            }

            using (var context = new AppDbContext())
            {
                var employeeCodes = string.Join(",", this.SelectedEmployees.Select(e => e.Code));

                // IN句を使用して複数の社員コードで検索
                var sql = $@"
                    SELECT
                        D.* 
                        , D物件担当.社員コード AS ChargeEmployeeCode
                    FROM
                        D物件 D 
                        INNER JOIN D物件担当 
                            ON D.連番 = D物件担当.物件連番 
                            AND D物件担当.社員コード IN ({employeeCodes})
                    WHERE
                        D.削除区分 = 0 
                        AND D.受注月度 = {202506}
                        ";

                var c = context.Database.SqlQueryRaw<ExtendedCaseInfo>(sql).ToList();
                if (c == null)
                {
                    this.ExtendedCaseInfos = new ObservableCollection<ExtendedCaseInfo>();
                }
                else
                {
                    this.ExtendedCaseInfos = new ObservableCollection<ExtendedCaseInfo>(c.OrderByDescending(c => c.Level));
                }
            }
        }

        private void SectionSelectionChangedExecute()
        {
            FetchEmployeeList();
            // 部署が変更されたら社員選択をクリア
            SelectedEmployees.Clear();
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

            FetchCaseList();

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

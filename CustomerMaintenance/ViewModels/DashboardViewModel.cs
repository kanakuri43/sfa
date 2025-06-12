using CustomerMaintenance.Models;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using sfa.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace CustomerMaintenance.ViewModels
{
    public class DashboardViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private ObservableCollection<Customer> _customers;
        private Section _selectedSection;
        private Employee _selectedEmployee;
        private ObservableCollection<Section> _sections;
        private ObservableCollection<Employee> _employees;

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

        public DelegateCommand SectionSelectionChanged { get; }
        public DelegateCommand EmployeeSelectionChanged { get; }

        public DashboardViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SectionSelectionChanged = new DelegateCommand(SectionSelectionChangedExecute);
            EmployeeSelectionChanged = new DelegateCommand(EmployeeSelectionChangedExecute);


            using (var context = new AppDbContext())
            {
                Sections = new ObservableCollection<Section>(
                    context.Sections.Where(s => s.State == 0).ToList()
                );
                this.SelectedSection = context.Sections.FirstOrDefault(s => s.Code == 11010);
            }

            FetchEmployeeList();

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

        private void SectionSelectionChangedExecute()
        {
            FetchEmployeeList();
        }
        private void EmployeeSelectionChangedExecute()
        {
            FetchCustomerList();
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

using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.CommonEnums;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ReportsParameters.Logistic
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class GeneralSalaryInfoReport : SingleUoWWidgetBase, IParametersWidget
    {
        public GeneralSalaryInfoReport(
            IEntityAutocompleteSelectorFactory employeeSelectorFactory,
            IInteractiveService interactiveService)
        {
            this.interactiveService = interactiveService ??
                                      throw new ArgumentNullException(nameof(interactiveService));
            this.employeeSelectorFactory = employeeSelectorFactory ??
                                      throw new ArgumentNullException(nameof(employeeSelectorFactory));
            this.Build();
            UoW = UnitOfWorkFactory.CreateWithoutRoot();
            Configure();
        }
        
        public string Title => "Основная информация по ЗП";
        public event EventHandler<LoadReportEventArgs> LoadReport;

        private readonly IEntityAutocompleteSelectorFactory employeeSelectorFactory;
        private readonly IInteractiveService interactiveService;

        private void Configure()
        {
            buttonRun.Sensitive = true;
            buttonRun.Clicked += OnButtonRunClicked;
            
            comboMonth.ItemsEnum = typeof(Month);
            comboMonth.SelectedItem = (Month)DateTime.Today.Month;

            comboYear.DefaultFirst = true;
            comboYear.ItemsList = Enumerable.Range(DateTime.Now.AddYears(-10).Year, 11).Reverse();
            
            comboCategory.ItemsEnum = typeof(EmployeeCategory);
            comboCategory.AddEnumToHideList(new Enum[] { EmployeeCategory.office });
            
            comboDriverOf.ItemsEnum = typeof(CarTypeOfUse);
            comboDriverOf.AddEnumToHideList(new Enum[] { CarTypeOfUse.CompanyTruck });
            
            entryEmployee.SetEntityAutocompleteSelectorFactory(employeeSelectorFactory);
            entryEmployee.Changed += (sender, args) => OnEmployeeSelected();
        }

        private void OnButtonRunClicked(object sender, EventArgs e)
        {
            LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo()));
        }

        private ReportInfo GetReportInfo()
        {
            var selectedYear = (int)comboYear.SelectedItem;
            var selectedMonth = (int)comboMonth.SelectedItem;
            var endDate = new DateTime(selectedYear, selectedMonth, DateTime.DaysInMonth(selectedYear, selectedMonth),
                23, 59, 59);
            
            return new ReportInfo {
                Identifier = "Logistic.GeneralSalaryInfoReport",
                Parameters = new Dictionary<string, object>
                {
                    { "start_date", new DateTime(selectedYear, selectedMonth, 1) },
                    { "end_date", endDate },
                    { "creation_date", DateTime.Now },
                    { "driver_of", comboDriverOf.SelectedItemOrNull },
                    { "employee_category", comboCategory.SelectedItemOrNull },
                    { "employee_id", entryEmployee.Subject?.GetIdOrNull() }
                }
            };
        }

        private void OnEmployeeSelected()
        {
            if (entryEmployee.Subject is Employee empl)
            {
                if (empl.Category == EmployeeCategory.office)
                {
                    interactiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя выбрать офисного сотрудника");
                    entryEmployee.Subject = null;
                    return;
                }
                if (empl.DriverOf == CarTypeOfUse.CompanyTruck)
                {
                    interactiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя выбрать водителя, управляющего фурой");
                    entryEmployee.Subject = null;
                    return;
                }
                comboDriverOf.Sensitive = false;
                comboDriverOf.SelectedItemOrNull = empl.DriverOf;
                comboCategory.Sensitive = false;
                comboCategory.SelectedItem = empl.Category;
            }
            else
            {
                comboDriverOf.Sensitive = true;
                comboCategory.Sensitive = true;
            }
        }
    }
}

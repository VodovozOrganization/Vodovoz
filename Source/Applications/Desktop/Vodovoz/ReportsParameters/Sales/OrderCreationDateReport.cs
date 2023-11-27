using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class OrderCreationDateReport : SingleUoWWidgetBase, IParametersWidget
	{
		public OrderCreationDateReport(INavigationManager navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var officeFilter = new EmployeeFilterViewModel();
			officeFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking);
			var employeeFactory = new EmployeeJournalFactory(navigationManager, officeFilter);
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			datePeriodPicker.PeriodChanged += (sender, e) => CanRun();
			buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по дате создания заказа";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object> {
				{ "start_date", datePeriodPicker.StartDateOrNull },
				{ "end_date", datePeriodPicker.EndDateOrNull },
				{ "employee_id", (evmeEmployee.Subject as Employee)?.Id ?? 0 }
			};

			return new ReportInfo {
				Identifier = "Sales.OrderCreationDateReport",
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		void CanRun()
		{
			buttonCreateReport.Sensitive = datePeriodPicker.EndDateOrNull.HasValue && datePeriodPicker.StartDateOrNull.HasValue;
		}
	}
}

using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModel;
using QS.Dialog.GtkUI;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class OrderCreationDateReport : SingleUoWWidgetBase, IParametersWidget
	{
		public OrderCreationDateReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var filter = new EmployeeRepresentationFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking
			);
			yEntRefEmployee.RepresentationModel = new EmployeesVM(filter);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по дате создания заказа";

		protected void OnButtonCreateReportEntered(object sender, EventArgs e) { }

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object> {
				{ "start_date", datePeriodPicker.StartDateOrNull },
				{ "end_date", datePeriodPicker.EndDateOrNull },
				{ "employee_id", (yEntRefEmployee.Subject as Employee)?.Id ?? 0 }
			};

			return new ReportInfo {
				Identifier = "Sales.OrderCreationDateReport",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e) => OnUpdate(true);

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		void CanRun()
		{
			buttonCreateReport.Sensitive = datePeriodPicker.EndDateOrNull.HasValue && datePeriodPicker.StartDateOrNull.HasValue;
		}

		protected void OnChangeReportParameters(object sender, EventArgs e) => CanRun();
	}
}
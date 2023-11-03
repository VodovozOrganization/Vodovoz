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

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MastersReport : SingleUoWWidgetBase, IParametersWidget
	{
		public MastersReport(INavigationManager navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var driverFilter = new EmployeeFilterViewModel();
			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver);
			var employeeFactory = new EmployeeJournalFactory(navigationManager, driverFilter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeDriver.Changed += (sender, e) => CanRun();
			dateperiodpicker.PeriodChanged += (sender, e) => CanRun();
			buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по выездным мастерам";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("start_date", dateperiodpicker.StartDateOrNull);
			parameters.Add("end_date", dateperiodpicker.EndDateOrNull);
			parameters.Add("driver_id", evmeDriver.SubjectId);

			return new ReportInfo {
				Identifier = "ServiceCenter.MastersReport",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		void CanRun()
		{
			buttonCreateReport.Sensitive = (dateperiodpicker.EndDateOrNull != null 
			                                && dateperiodpicker.StartDateOrNull != null 
			                                && evmeDriver.Subject != null);
		}
	}
}

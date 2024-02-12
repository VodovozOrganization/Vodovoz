using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DefectiveItemsReport : SingleUoWWidgetBase, IParametersWidget
	{
		public DefectiveItemsReport(INavigationManager navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			yEnumCmbSource.ItemsEnum = typeof(DefectSource);
			yEnumCmbSource.AddEnumToHideList(new Enum[] { DefectSource.None });

			var driverFilter = new EmployeeFilterViewModel();
			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver);
			var employeeFactory = new EmployeeJournalFactory(navigationManager, driverFilter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());

			datePeriod.StartDate = datePeriod.EndDate = DateTime.Today;
			buttonRun.Clicked += (sender, e) => OnUpdate(true);
			datePeriod.PeriodChanged += (sender, e) => ValidateParameters();
		}

		#region IParametersWidget implementation

		public string Title => "Отчёт по браку";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		private ReportInfo GetReportInfo()
		{
			var driver = 0;
			if(evmeDriver.Subject is Employee)
				driver = evmeDriver.SubjectId;
			var source = yEnumCmbSource.SelectedItem;
			var startDate = datePeriod.StartDateOrNull.Value.ToString("yyyy-MM-dd");
			var endDate = datePeriod.EndDateOrNull.Value.ToString("yyyy-MM-dd");

			var repInfo = new ReportInfo {
				Identifier = "Store.DefectiveItemsReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", startDate },
					{ "end_date", endDate },
					{ "source", source },
					{ "driver_id", driver }
				}
			};

			return repInfo;
		}

		void ValidateParameters()
		{
			var datePeriodSelected = datePeriod.EndDateOrNull != null && datePeriod.StartDateOrNull != null;
			buttonRun.Sensitive = datePeriodSelected;
		}
	}
}

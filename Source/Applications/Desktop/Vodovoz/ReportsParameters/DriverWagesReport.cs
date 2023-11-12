using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using QS.Navigation;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverWagesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly INavigationManager _navigationManager;

		public DriverWagesReport(INavigationManager navigationManager)
		{
			this.Build();
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var driverFilter = new EmployeeFilterViewModel();
			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver);
			var employeeFactory = new EmployeeJournalFactory(_navigationManager, driverFilter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeDriver.Changed += (sender, args) =>
			{
				if(dateperiodpicker.StartDateOrNull.HasValue && evmeDriver.Subject is Employee)
					OnUpdate(true);
			};
			
			dateperiodpicker.PeriodChanged += (sender, args) =>
			{
				if(evmeDriver.Subject is Employee && dateperiodpicker.StartDateOrNull.HasValue)
					OnUpdate(true);
			};
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Зарплата водителя";

		#endregion

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		private ReportInfo GetReportInfo()
		{
			var endDate = dateperiodpicker.EndDateOrNull;
			if(endDate != null)
			{
				endDate = endDate.GetValueOrDefault().AddHours(23).AddMinutes(59);
			}

			var parameters = new Dictionary<string, object>
				{
					{ "driver_id", evmeDriver.SubjectId },
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", endDate }
			};

			if(checkShowBalance.Active) {
				parameters.Add("showbalance", "1");
			} else {
				parameters.Add("showbalance", "0");
			}
			return new ReportInfo
			{
				Identifier = "Wages.DriverWage",
				Parameters = parameters
			};
		}	

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			if (!(evmeDriver.Subject is Employee))
			{
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать водителя");
				return;
			}
			if(dateperiodpicker.StartDateOrNull == null)
			{
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать дату");
				return;
			}
			OnUpdate(true);
		}
	}
}

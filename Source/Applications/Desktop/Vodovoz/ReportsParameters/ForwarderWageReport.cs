using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QSReport;
using Vodovoz.Core.Domain.Employees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vodovoz.Domain.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using QS.Project.Services;

namespace Vodovoz.Reports
{
	[ToolboxItem(true)]
	public partial class ForwarderWageReport : SingleUoWWidgetBase, IParametersWidget
	{
		private const bool _showFinesOutsidePeriodDefault = false;

		public ForwarderWageReport(INavigationManager navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			Build();

			ycheckbuttonShowFinesOutsidePeriod.Active = _showFinesOutsidePeriodDefault;

			var forwarderFilter = new EmployeeFilterViewModel();
			forwarderFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.forwarder);

			var employeeFactory = new EmployeeJournalFactory(navigationManager, forwarderFilter);

			evmeForwarder.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());

			evmeForwarder.Changed += (sender, e) => RefreshSensitivity();
			dateperiodpicker.PeriodChanged += (sender, e) => RefreshSensitivity();
			buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по зарплате экспедитора";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "show_fines_outside_period", ycheckbuttonShowFinesOutsidePeriod.Active },
				{ "forwarder_id", evmeForwarder.SubjectId }
			};

			if(checkShowBalance.Active)
			{
				parameters.Add("showbalance", "1");
			}
			else
			{
				parameters.Add("showbalance", "0");
			}

			return new ReportInfo
			{
				Identifier = "Employees.ForwarderWage",
				Parameters = parameters
			};
		}

		private void OnUpdate(bool hide = false)
		{
			if(LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		private void RefreshSensitivity()
		{
			buttonCreateReport.Sensitive = CanGenerate;
		}

		public bool CanGenerate =>
			dateperiodpicker.EndDateOrNull != null
			&& dateperiodpicker.StartDateOrNull != null
			&& evmeForwarder.Subject != null;
	}
}

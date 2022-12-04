using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Reports;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MastersVisitReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public MastersVisitReport(ReportFactory reportFactory, IEmployeeJournalFactory employeeJournalFactory)
		{
			if(employeeJournalFactory == null)
			{
				throw new ArgumentNullException(nameof(employeeJournalFactory));
			}
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));

			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			var driversFilter = new EmployeeFilterViewModel();
			driversFilter.SetAndRefilterAtOnce(
				x => x.Category = EmployeeCategory.driver,
				x => x.Status = null);
			employeeJournalFactory.SetEmployeeFilterViewModel(driversFilter);
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчёт по выездам мастеров";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "master", evmeEmployee.SubjectId }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "ServiceCenter.MastersVisitReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null)
			{
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать дату");
				return;
			}

			if(evmeEmployee.Subject == null)
			{
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать сотрудника");
				return;
			}
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}
	}
}

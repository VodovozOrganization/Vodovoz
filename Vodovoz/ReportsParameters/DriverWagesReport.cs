using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;
using QS.Dialog.GtkUI;
using Vodovoz.JournalFilters;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverWagesReport : SingleUoWWidgetBase, IParametersWidget
	{
		public DriverWagesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			var filter = new EmployeeRepresentationFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			yentryreferenceDriver.RepresentationModel = new EmployeesVM(filter);
			yentryreferenceDriver.Changed += (sender, args) =>
			{
				if(dateperiodpicker.StartDateOrNull.HasValue && yentryreferenceDriver.Subject is Employee)
					OnUpdate(true);
			};
			
			dateperiodpicker.PeriodChanged += (sender, args) =>
			{
				if(yentryreferenceDriver.Subject is Employee && dateperiodpicker.StartDateOrNull.HasValue)
					OnUpdate(true);
			};
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title	{ 
			get {
				return "Зарплата водителя";
			}
		}

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
					{ "driver_id", (yentryreferenceDriver.Subject as Employee).Id},
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
			if ((yentryreferenceDriver.Subject as Employee) == null)
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


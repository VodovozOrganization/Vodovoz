using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;
using Vodovoz.Filters.ViewModels;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MastersReport : SingleUoWWidgetBase, IParametersWidget
	{
		public MastersReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var filter = new EmployeeFilterViewModel(QS.Project.Services.ServicesConfig.CommonServices);
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.ShowFired = false
			);
			yentryreferenceDriver.RepresentationModel = new EmployeesVM(filter);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по выездным мастерам";
			}
		}

		protected void OnButtonCreateReportEntered(object sender, EventArgs e)
		{
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("start_date", dateperiodpicker.StartDateOrNull);
			parameters.Add("end_date", dateperiodpicker.EndDateOrNull);
			parameters.Add("driver_id", (yentryreferenceDriver.Subject as Employee).Id);

			return new ReportInfo {
				Identifier = "ServiceCenter.MastersReport",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		void CanRun()
		{
			buttonCreateReport.Sensitive = (dateperiodpicker.EndDateOrNull != null 
			                                && dateperiodpicker.StartDateOrNull != null 
			                                && yentryreferenceDriver.Subject != null);
		}

		protected void OnDateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			CanRun();
		}

		protected void OnYentryreferenceDriverChanged(object sender, EventArgs e)
		{
			CanRun();
		}
	}
}

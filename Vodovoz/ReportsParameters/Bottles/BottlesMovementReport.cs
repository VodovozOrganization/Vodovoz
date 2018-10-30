using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Report;
using QSProjectsLib;
using QSReport;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottlesMovementReport : Gtk.Bin, IEntityDialog, IParametersWidget
	{
		public BottlesMovementReport()
		{
			this.Build();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get {
				return null;
			}
		}

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчёт по движению бутылей (по МЛ)";
			}
		}

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Bottles.BottlesMovementReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull }
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null) {
				MessageDialogWorks.RunErrorDialog("Необходимо выбрать период");
				return;
			}
			OnUpdate(true);
		}
	}
}

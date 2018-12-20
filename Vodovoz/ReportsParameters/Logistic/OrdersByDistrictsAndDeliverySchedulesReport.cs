using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersByDistrictsAndDeliverySchedulesReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public IUnitOfWork UoW { get; private set; }
		DateTime date;
		public OrdersByDistrictsAndDeliverySchedulesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			date = pkrDate.Date = DateTime.Today.AddDays(1);
		}

		#region IParametersWidget implementation

		public string Title => "Заказы по районам и интервалам";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null)
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Logistic.OrdersByDistrictsAndDeliverySchedules",
				Parameters = new Dictionary<string, object> {
					{ "date", date }
				}
			};
		}

		protected void OnPkrDateDateChangedByUser(object sender, EventArgs e)
		{
			date = pkrDate.Date;
		}
	}
}

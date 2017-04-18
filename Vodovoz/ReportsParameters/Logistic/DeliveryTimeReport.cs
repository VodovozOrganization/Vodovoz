using System;
using System.Collections.Generic;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReport : Gtk.Bin, IParametersWidget
	{
		public DeliveryTimeReport ()
		{
			this.Build ();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет 'Время доставки'";
			}
		}

		#endregion

		void OnUpdate (bool hide = false)
		{
			if (LoadReport != null) {
				LoadReport (this, new LoadReportEventArgs (GetReportInfo (), hide));
			}
		}

		private ReportInfo GetReportInfo ()
		{
			return new ReportInfo {
				Identifier = "Logistic.DeliveryTime",
				Parameters = new Dictionary<string, object>
				{
					{ "beforeTime", ytimeDelivery.Text },
				}
			};
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate (true);
		}

		protected void OnYtimeDeliveryChanged (object sender, EventArgs e)
		{
			buttonCreateReport.Sensitive = ytimeDelivery.Time != default (TimeSpan);
		}
	}
}

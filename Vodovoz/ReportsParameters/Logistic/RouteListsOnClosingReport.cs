using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListsOnClosingReport : Gtk.Bin, IParametersWidget
	{
		public RouteListsOnClosingReport()
		{
			this.Build();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title
		{
			get
			{
				return "Отчет по незакрытым МЛ";
			}
		}

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();
			if(buttonToDayRL.Active)
				parameters.Add("todayRloff", 1);
			else
				parameters.Add("todayRloff", 0);
			
			return new ReportInfo
			{
				Identifier = "Logistic.RouteListOnClosing",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}

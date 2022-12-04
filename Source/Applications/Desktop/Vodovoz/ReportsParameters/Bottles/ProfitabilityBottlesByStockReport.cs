using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Bottles
{
	public partial class ProfitabilityBottlesByStockReport : Gtk.Bin, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		class PercentNode
		{
			public PercentNode(int pct) => Pct = pct;

			public int Pct { get; set; }
			public string Name => string.Format("{0}%", Pct);
		}

		public ProfitabilityBottlesByStockReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			dtrngPeriod.StartDate = DateTime.Today;
			dtrngPeriod.EndDate = DateTime.Today;
			specCmbDiscountPct.ItemsList = new List<PercentNode> {
				new PercentNode(0),
				new PercentNode(10),
				new PercentNode(20)
			};
			specCmbDiscountPct.SetRenderTextFunc<PercentNode>(x => x.Name);
		}

		#region IParametersWidget implementation

		public string Title => "Рентабельность акции \"Бутыль\"";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion IParametersWidget implementation

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		protected void OnButtonRunClicked(object sender, EventArgs e) => OnUpdate(true);

		ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dtrngPeriod.StartDate },
				{ "end_date", dtrngPeriod.EndDate },
				{ "discount_stock", (specCmbDiscountPct.SelectedItem as PercentNode)?.Pct ?? -1}
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Bottles.ProfitabilityBottlesByStock";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}
	}
}

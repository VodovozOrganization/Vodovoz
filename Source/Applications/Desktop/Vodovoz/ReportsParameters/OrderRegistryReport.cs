using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderRegistryReport : SingleUoWWidgetBase, IParametersWidget
	{
		GenericObservableList<GeoGroup> geographicGroups;
		private readonly ReportFactory _reportFactory;

		public OrderRegistryReport(ReportFactory reportFactory)
		{
			this.Build();
			ydatepicker.Date = DateTime.Now.Date;
			ConfigureDlg();
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
		}

		void ConfigureDlg()
		{
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			geograficGroup.UoW = UoW;
			geograficGroup.Label = "Часть города:";
			geographicGroups = new GenericObservableList<GeoGroup>();
			geograficGroup.Items = geographicGroups;
			foreach(var gg in UoW.Session.QueryOver<GeoGroup>().List())
				geographicGroups.Add(gg);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Реестр заказов";

		#endregion

		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "date", ydatepicker.Date },
				{ "geographic_groups", GetResultIds(geographicGroups.Select(g => g.Id)) }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Orders.OrderRegistryReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e) => OnUpdate(true);
	}
}

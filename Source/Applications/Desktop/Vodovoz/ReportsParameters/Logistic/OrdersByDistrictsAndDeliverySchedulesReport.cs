using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.Domain.Sale;
using QS.Project.Services;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersByDistrictsAndDeliverySchedulesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public OrdersByDistrictsAndDeliverySchedulesReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			pkrDate.StartDate = pkrDate.EndDate = DateTime.Today;
			lstGeographicGroup.ItemsList = UoW.GetAll<GeoGroup>();
			lstGeographicGroup.SetRenderTextFunc<GeoGroup>(x => x.Name);
			yspeccomboboxTariffZone.SetRenderTextFunc<TariffZone>(x => x.Name);
			yspeccomboboxTariffZone.ItemsList = UoW.GetAll<TariffZone>().OrderBy(x => x.Name);
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
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", pkrDate.StartDate },
				{ "end_date", pkrDate.EndDate },
				{ "geographic_group_id",(lstGeographicGroup.SelectedItem as GeoGroup)?.Id },
				{ "geographic_group_name",(lstGeographicGroup.SelectedItem as GeoGroup)?.Name },
				{ "tariff_zone_id",(yspeccomboboxTariffZone.SelectedItem as TariffZone)?.Id },
				{ "tariff_zone_name",(yspeccomboboxTariffZone.SelectedItem as TariffZone)?.Name }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Logistic.OrdersByDistrictsAndDeliverySchedules";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}
	}
}

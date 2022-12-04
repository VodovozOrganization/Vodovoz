using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Retail;
using Vodovoz.Domain.Sale;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Retail
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public CounterpartyReport(
			ReportFactory reportFactory,
			IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
			IEntityAutocompleteSelectorFactory districtSelectorFactory,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			this.Build();
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			ConfigureView(salesChannelSelectorFactory, districtSelectorFactory);
		}

		public string Title => $"Отчет по контрагентам розницы";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private void ConfigureView(IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
			IEntityAutocompleteSelectorFactory districtSelectorFactory)
		{
			buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);
			yEntitySalesChannel.SetEntityAutocompleteSelectorFactory(salesChannelSelectorFactory);
			yEntityDistrict.SetEntityAutocompleteSelectorFactory(districtSelectorFactory);
			yenumPaymentType.ItemsEnum = typeof(PaymentType);
			yenumPaymentType.SelectedItem = PaymentType.cash;
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object> {
				{ "create_date", ydateperiodpickerCreate.StartDateOrNull },
				{ "end_date", ydateperiodpickerCreate.EndDateOrNull?.AddDays(1).AddSeconds(-1) },
				{ "sales_channel_id", (yEntitySalesChannel.Subject as SalesChannel)?.Id ?? 0},
				{ "district", (yEntityDistrict.Subject as District)?.Id ?? 0 },
				{ "payment_type", (yenumPaymentType.SelectedItemOrNull)},
				{ "all_types", (ycheckpaymentform.Active)}
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Retail.CounterpartyReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}
	}
}

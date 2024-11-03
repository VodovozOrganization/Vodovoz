using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Retail;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ReportsParameters.Retail
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IDistrictJournalFactory _districtJournalFactory;
		private readonly IInteractiveService _interactiveService;

		public CounterpartyReport(
			IReportInfoFactory reportInfoFactory,
			ISalesChannelJournalFactory salesChannelJournalFactory,
			IDistrictJournalFactory districtJournalFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_districtJournalFactory = districtJournalFactory ?? throw new ArgumentNullException(nameof(districtJournalFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			Build();

			UoW = unitOfWorkFactory.CreateWithoutRoot();

			ConfigureView(salesChannelJournalFactory.CreateSalesChannelAutocompleteSelectorFactory(), _districtJournalFactory.CreateDistrictAutocompleteSelectorFactory());
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
			yenumPaymentType.SelectedItem = PaymentType.Cash;
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

			var reportInfo = _reportInfoFactory.Create("Retail.CounterpartyReport", Title, parameters);
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

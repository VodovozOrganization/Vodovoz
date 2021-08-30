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
using Vodovoz.Domain.Sectors;

namespace Vodovoz.ReportsParameters.Retail
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class CounterpartyReport : SingleUoWWidgetBase, IParametersWidget
    {
        private readonly IInteractiveService interactiveService;
        
        public CounterpartyReport(IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
            IEntityAutocompleteSelectorFactory sectorsSelectorFactory, IUnitOfWorkFactory unitOfWorkFactory,
            IInteractiveService interactiveService)
        {
            this.Build();
            this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            UoW = unitOfWorkFactory.CreateWithoutRoot();
            ConfigureView(salesChannelSelectorFactory, sectorsSelectorFactory);
        }

        public string Title => $"Отчет по контрагентам розницы";

        public event EventHandler<LoadReportEventArgs> LoadReport;

        private void ConfigureView(IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
            IEntityAutocompleteSelectorFactory sectorsSelectorFactory)
        {
            buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);
            yEntitySalesChannel.SetEntityAutocompleteSelectorFactory(salesChannelSelectorFactory);
            yEntityDistrict.SetEntityAutocompleteSelectorFactory(sectorsSelectorFactory);
            yenumPaymentType.ItemsEnum = typeof(PaymentType);
            yenumPaymentType.SelectedItem = PaymentType.cash;
        }

        private ReportInfo GetReportInfo()
        {
                var parameters = new Dictionary<string, object> {
                { "create_date", ydateperiodpickerCreate.StartDateOrNull },
                { "end_date", ydateperiodpickerCreate.EndDateOrNull?.AddDays(1).AddSeconds(-1) },
                { "sales_channel_id", (yEntitySalesChannel.Subject as SalesChannel)?.Id ?? 0},
                { "sector", (yEntityDistrict.Subject as Sector)?.Id ?? 0 },
                { "payment_type", (yenumPaymentType.SelectedItemOrNull)},
                { "all_types", (ycheckpaymentform.Active)}
            };
                return new ReportInfo
            {
                Identifier = "Retail.CounterpartyReport",
                Parameters = parameters
            };
        }

        void OnUpdate(bool hide = false)
        {
            LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
        }
    }
}

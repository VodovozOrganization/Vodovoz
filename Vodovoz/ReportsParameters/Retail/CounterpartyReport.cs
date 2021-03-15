using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Retail;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters.Retail
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class CounterpartyReport : SingleUoWWidgetBase, IParametersWidget
    {
        public CounterpartyReport(IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
            IEntityAutocompleteSelectorFactory districtSelectorFactory)
        {
            this.Build();
            ConfigureView(salesChannelSelectorFactory, districtSelectorFactory);
        }

        public string Title => $"Отчет по контрагентам розницы";

        public event EventHandler<LoadReportEventArgs> LoadReport;

        private void ConfigureView(IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
            IEntityAutocompleteSelectorFactory districtSelectorFactory)
        {
            buttonCreateReport.Clicked += (sender, e) => Validate();
            yEntitySalesChannel.SetEntityAutocompleteSelectorFactory(salesChannelSelectorFactory);
            yEntityDistrict.SetEntityAutocompleteSelectorFactory(districtSelectorFactory);
            yenumPaymentType.ItemsEnum = typeof(PaymentType);
        }

        private ReportInfo GetReportInfo()
        {
                var parameters = new Dictionary<string, object> {
                { "create_date", ydateperiodpickerCreate.StartDateOrNull },
                { "sales_channel_id", (yEntitySalesChannel.Subject as SalesChannel)?.Id ?? 0},
                { "district", (yEntityDistrict.Subject as District)?.Id ?? 0 },
                { "payment_type", ((int)yenumPaymentType.SelectedItemOrNull)}
            };

            return new ReportInfo
            {
                Identifier = "Retail.CounterpartyReport",
                Parameters = parameters
            };
        }

        void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

        void Validate()
        {
            string errorString = string.Empty;
            if (!ydateperiodpickerCreate.StartDateOrNull.HasValue &&
                !ydateperiodpickerCreate.EndDateOrNull.HasValue)
            {
                errorString = "Не выбран ни один из фильтров дат";
                MessageDialogHelper.RunErrorDialog(errorString);
                return;
            }
            OnUpdate(true);
        }
    }
}

using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Retail;

namespace Vodovoz.ReportsParameters.Retail
{
    public partial class QualityReport : SingleUoWWidgetBase, IParametersWidget
    {
        public QualityReport(IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
            IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
            IEntityAutocompleteSelectorFactory employeeSelectorFactory)
        {
            this.Build();
            Configure(counterpartySelectorFactory, salesChannelSelectorFactory, employeeSelectorFactory);
        }

        private void Configure(IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
            IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
            IEntityAutocompleteSelectorFactory employeeSelectorFactory)
        {
            buttonCreateReport.Clicked += (sender, e) => Validate();
            yEntityCounterParty.SetEntityAutocompleteSelectorFactory(counterpartySelectorFactory);
            yEntitySalesChannel.SetEntityAutocompleteSelectorFactory(salesChannelSelectorFactory);
            yEntityMainContact.SetEntityAutocompleteSelectorFactory(employeeSelectorFactory);
        }

        private ReportInfo GetReportInfo()
        {
            var parameters = new Dictionary<string, object> {
                { "create_date", ydateperiodpickerCreate.StartDateOrNull },
                { "shipping_date", ydateperiodpickerShippind.StartDateOrNull },
                { "counterparty_id", (yEntityCounterParty.Subject as Counterparty)?.Id ?? 0 },
                { "sales_channel_id", (yEntitySalesChannel.Subject as SalesChannel)?.Id ?? 0},
                { "main_contact_id", (yEntityMainContact.Subject as Employee)?.Id ?? 0}
            };

            return new ReportInfo
            {
                Identifier = "Retail.QualityReport",
                Parameters = parameters
            };
        }

        public string Title => $"Качественный отчет";

        public event EventHandler<LoadReportEventArgs> LoadReport;

        void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

        void Validate()
        {
            string errorString = string.Empty;
            if (!string.IsNullOrWhiteSpace(errorString))
            {
                MessageDialogHelper.RunErrorDialog(errorString);
                return;
            }
            OnUpdate(true);
        }
    }
}

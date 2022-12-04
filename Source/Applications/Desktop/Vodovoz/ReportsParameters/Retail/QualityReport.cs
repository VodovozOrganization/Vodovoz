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
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Retail;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Retail
{
    public partial class QualityReport : SingleUoWWidgetBase, IParametersWidget
    {
		private readonly ReportFactory _reportFactory;
		private readonly IInteractiveService interactiveService;
        
        public QualityReport(
			ReportFactory reportFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
            IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
            IEntityAutocompleteSelectorFactory employeeSelectorFactory,
            IUnitOfWorkFactory unitOfWorkFactory,
            IInteractiveService interactiveService)
        {
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            this.Build();
            UoW = unitOfWorkFactory.CreateWithoutRoot();
            Configure(counterpartySelectorFactory, salesChannelSelectorFactory, employeeSelectorFactory);
        }

        private void Configure(IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
            IEntityAutocompleteSelectorFactory salesChannelSelectorFactory,
            IEntityAutocompleteSelectorFactory employeeSelectorFactory)
        {
            buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);
            yEntityCounterParty.SetEntityAutocompleteSelectorFactory(counterpartySelectorFactory);
            yEntitySalesChannel.SetEntityAutocompleteSelectorFactory(salesChannelSelectorFactory);
            yEntityMainContact.SetEntityAutocompleteSelectorFactory(employeeSelectorFactory);
        }

        private ReportInfo GetReportInfo()
        {
            var parameters = new Dictionary<string, object> {
                { "create_date", ydateperiodpickerCreate.StartDateOrNull },
                { "end_date", ydateperiodpickerCreate.EndDateOrNull?.AddDays(1).AddSeconds(-1) },
                { "shipping_start_date", ydateperiodpickerShipping.StartDateOrNull },
                { "shipping_end_date", ydateperiodpickerShipping.EndDateOrNull?.AddDays(1).AddSeconds(-1) },
                { "counterparty_id", ((Counterparty)yEntityCounterParty.Subject)?.Id ?? 0 },
                { "sales_channel_id", ((SalesChannel)yEntitySalesChannel.Subject)?.Id ?? 0},
                { "main_contact_id", ((Employee)yEntityMainContact.Subject)?.Id ?? 0}
            };

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Retail.QualityReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
        }

        public string Title => $"Качественный отчет";

        public event EventHandler<LoadReportEventArgs> LoadReport;

        void OnUpdate(bool hide = false)
        {
            if (Validate())
            {
                LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
            }
        }

        bool Validate()
        {
            string errorString = string.Empty;
            if (!(ydateperiodpickerCreate.StartDateOrNull.HasValue &&
                ydateperiodpickerCreate.EndDateOrNull.HasValue) &&
                !(ydateperiodpickerShipping.StartDateOrNull.HasValue && 
                  ydateperiodpickerShipping.EndDateOrNull.HasValue))
            {
                errorString = "Не выбраны периоды";
                interactiveService.ShowMessage(ImportanceLevel.Error, errorString);
                return false;
            }

            return true;
        }
    }
}

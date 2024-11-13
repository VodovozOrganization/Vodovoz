using Autofac;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Retail;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ReportsParameters.Retail
{
	public partial class QualityReport : SingleUoWWidgetBase, IParametersWidget
    {
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ISalesChannelJournalFactory _salesChannelJournalFactory;
		private readonly IInteractiveService _interactiveService;
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
        
        public QualityReport(
			IReportInfoFactory reportInfoFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ISalesChannelJournalFactory salesChannelJournalFactory,
            IUnitOfWorkFactory unitOfWorkFactory,
            IInteractiveService interactiveService)
        {
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_salesChannelJournalFactory = salesChannelJournalFactory ?? throw new ArgumentNullException(nameof(salesChannelJournalFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            
			Build();
			
            UoW = unitOfWorkFactory.CreateWithoutRoot();
            Configure(
				_counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope),
				_salesChannelJournalFactory.CreateSalesChannelAutocompleteSelectorFactory(),
				_employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());
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

			var reportInfo = _reportInfoFactory.Create("Retail.QualityReport", Title, parameters);
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
                _interactiveService.ShowMessage(ImportanceLevel.Error, errorString);
                return false;
            }

            return true;
        }

		protected override void OnDestroyed()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.OnDestroyed();
		}
	}
}

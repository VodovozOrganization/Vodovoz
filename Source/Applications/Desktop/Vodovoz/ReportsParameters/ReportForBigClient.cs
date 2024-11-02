using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ReportsParameters
{
	public partial class ReportForBigClient : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel;
		private readonly IReportInfoFactory _reportInfoFactory;

		public ReportForBigClient(ILifetimeScope lifetimeScope, IReportInfoFactory reportInfoFactory)
		{
			if(lifetimeScope == null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));

			Build();
			var uowFactory = lifetimeScope.Resolve<IUnitOfWorkFactory>();
			UoW = uowFactory.CreateWithoutRoot();
			_counterpartyJournalFactory = new CounterpartyJournalFactory();
			_deliveryPointJournalFilterViewModel = new DeliveryPointJournalFilterViewModel();
			_deliveryPointJournalFactory = lifetimeScope.Resolve<IDeliveryPointJournalFactory>();
			_deliveryPointJournalFactory.SetDeliveryPointJournalFilterViewModel(_deliveryPointJournalFilterViewModel);
			evmeCounterparty
				.SetEntityAutocompleteSelectorFactory(_counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope));
			evmeCounterparty.Changed += OnCounterpartyChanged;

			evmeDeliveryPoint
				.SetEntityAutocompleteSelectorFactory(_deliveryPointJournalFactory
					.CreateDeliveryPointByClientAutocompleteSelectorFactory());
		}

		#region IParametersWidget implementation

		public string Title => "Отчёт \"Куньголово\"";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "startDate", dateperiodpicker1.StartDateOrNull },
				{ "endDate", dateperiodpicker1.EndDateOrNull },
				{ "client_id", evmeCounterparty.SubjectId },
				{ "delivery_point_id", evmeDeliveryPoint.Subject == null ? -1 : evmeDeliveryPoint.SubjectId }
			};

			var reportInfo = _reportInfoFactory.Create("Client.SummaryBottlesAndDepositsKungolovo", Title, parameters);
			return reportInfo;
		}

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			buttonRun.Sensitive = evmeCounterparty.Subject != null;
		}

		private void OnCounterpartyChanged(object sender, EventArgs e)
		{
			ValidateParameters();
			if(evmeCounterparty.Subject == null)
			{
				evmeDeliveryPoint.Subject = null;
				evmeDeliveryPoint.Sensitive = false;
			}
			else
			{
				_deliveryPointJournalFilterViewModel.Counterparty = evmeCounterparty.Subject as Counterparty;
				evmeDeliveryPoint.Subject = null;
				evmeDeliveryPoint.Sensitive = true;
			}
		}
	}
}

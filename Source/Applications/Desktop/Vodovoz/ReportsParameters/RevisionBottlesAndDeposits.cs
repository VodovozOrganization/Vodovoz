using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.Reports
{
	public partial class RevisionBottlesAndDeposits : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilter = new DeliveryPointJournalFilterViewModel();
		//Т.к. отчет открывается из диалога звонка, то мы не можем контролировать время жизни скоупа
		//Поэтому создаем на месте
		private ILifetimeScope _lifetimeScope = ScopeProvider.Scope.BeginLifetimeScope();
		private bool _showStockBottle;

		public RevisionBottlesAndDeposits(
			IReportInfoFactory reportInfoFactory,
			IOrderRepository orderRepository,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			
			Build();
			var uowFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();
			UoW = uowFactory.CreateWithoutRoot();
			entityViewModelEntryCounterparty
				.SetEntityAutocompleteSelectorFactory(
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
			(deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory)))
				.SetDeliveryPointJournalFilterViewModel(_deliveryPointJournalFilter);
			evmeDeliveryPoint
				.SetEntityAutocompleteSelectorFactory(deliveryPointJournalFactory.CreateDeliveryPointByClientAutocompleteSelectorFactory());
		}

		public void SetDeliveryPoint(DeliveryPoint deliveryPoint)
		{
			entityViewModelEntryCounterparty.Subject = deliveryPoint.Counterparty;
			evmeDeliveryPoint.Subject = deliveryPoint;
		}

		public void SetCounterparty(Counterparty counterparty)
		{
			entityViewModelEntryCounterparty.Subject = counterparty;
		}

		#region IParametersWidget implementation

		public string Title => "Акт по бутылям-залогам";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		public void OnUpdate(bool hide = false)
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
				{ "startDate", new DateTime(2000,1,1) },
				{ "endDate", DateTime.Today.AddYears(1) },
				{ "client_id", entityViewModelEntryCounterparty.GetSubject<Counterparty>().Id},
				{ "delivery_point_id", evmeDeliveryPoint.Subject == null ? -1 : evmeDeliveryPoint.SubjectId},
				{ "show_stock_bottle", _showStockBottle }
			};

			var reportInfo = _reportInfoFactory.Create("Client.SummaryBottlesAndDeposits", Title, parameters);
			return reportInfo;
		}

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			var counterpartySelected = entityViewModelEntryCounterparty.Subject != null;
			buttonRun.Sensitive = counterpartySelected;
		}

		protected void OnEntryCounterpartyChanged (object sender, EventArgs e)
		{
			ValidateParameters();

			if(entityViewModelEntryCounterparty.Subject == null)
			{
				evmeDeliveryPoint.Subject = null;
				evmeDeliveryPoint.Sensitive = false;
			}
			else
			{
				_showStockBottle = _orderRepository.IsBottleStockExists(UoW, entityViewModelEntryCounterparty.GetSubject<Counterparty>());
				evmeDeliveryPoint.Subject = null;
				evmeDeliveryPoint.Sensitive = true;
				_deliveryPointJournalFilter.Counterparty = entityViewModelEntryCounterparty.Subject as Counterparty;
			}
		}

		public override void Destroy()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}

			base.Destroy();
		}
	}
}

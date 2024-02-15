using System;
using System.Collections.Generic;
using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.Reports
{
	public partial class RevisionBottlesAndDeposits : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IOrderRepository _orderRepository;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilter = new DeliveryPointJournalFilterViewModel();
		private bool _showStockBottle;

		public RevisionBottlesAndDeposits(
			ILifetimeScope lifetimeScope,
			IOrderRepository orderRepository,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory)
		{
			if(lifetimeScope == null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			
			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			entityViewModelEntryCounterparty
				.SetEntityAutocompleteSelectorFactory(
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope));
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
			return new ReportInfo
			{
				Identifier = "Client.SummaryBottlesAndDeposits",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", new DateTime(2000,1,1) },
					{ "endDate", DateTime.Today.AddYears(1) },
					{ "client_id", entityViewModelEntryCounterparty.GetSubject<Counterparty>().Id},
					{ "delivery_point_id", evmeDeliveryPoint.Subject == null ? -1 : evmeDeliveryPoint.SubjectId},
					{ "show_stock_bottle", _showStockBottle }
				}
			};
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
	}
}

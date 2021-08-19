using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Reports
{
	public partial class RevisionBottlesAndDeposits : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IOrderRepository _orderRepository;
		public bool ShowStockBottle { get; set; }

		public RevisionBottlesAndDeposits(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			
			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			entityViewModelEntryCounterparty.SetEntityAutocompleteSelectorFactory(new DefaultEntityAutocompleteSelectorFactory<Counterparty,
				CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices));
		}	

		public void SetDeliveryPoint(DeliveryPoint deliveryPoint)
		{
			entityViewModelEntryCounterparty.Subject = deliveryPoint.Counterparty;
			referenceDeliveryPoint.Subject = deliveryPoint;
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
					{ "delivery_point_id", referenceDeliveryPoint.Subject == null ? -1 : referenceDeliveryPoint.GetSubject<DeliveryPoint>().Id},
					{ "show_stock_bottle", ShowStockBottle }
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
				referenceDeliveryPoint.Subject = null;
				referenceDeliveryPoint.Sensitive = false;
			}
			else
			{
				ShowStockBottle = _orderRepository.IsBottleStockExists(UoW, entityViewModelEntryCounterparty.GetSubject<Counterparty>());
				referenceDeliveryPoint.Subject = null;
				referenceDeliveryPoint.Sensitive = true;
				referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, 
					entityViewModelEntryCounterparty.GetSubject<Counterparty>());
			}
		}
	}
}


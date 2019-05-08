using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Repositories.Orders;

namespace Vodovoz.Reports
{
	public partial class RevisionBottlesAndDeposits : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public IUnitOfWork UoW { get; private set;}

		public bool ShowStockBottle { get; set; }

		public RevisionBottlesAndDeposits()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			referenceCounterparty.RepresentationModel = new ViewModel.CounterpartyVM(UoW);
		}	

		public void SetDeliveryPoint(DeliveryPoint deliveryPoint)
		{
			referenceCounterparty.Subject = deliveryPoint.Counterparty;
			referenceDeliveryPoint.Subject = deliveryPoint;
		}

		#region IParametersWidget implementation

		public string Title
		{
			get
			{
				return "Акт по бутылям-залогам";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
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
					{ "startDate", dateperiodpicker1.StartDateOrNull },
					{ "endDate", dateperiodpicker1.EndDateOrNull },
					{ "client_id", referenceCounterparty.GetSubject<Counterparty>().Id},
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
			var counterpartySelected = referenceCounterparty.Subject != null;
			buttonRun.Sensitive = counterpartySelected;
		}

		protected void OnReferenceCounterpartyChanged (object sender, EventArgs e)
		{
			ValidateParameters();
			ShowStockBottle = OrderRepository.IsBottleStockExists(UoW, referenceCounterparty.GetSubject<Counterparty>());

			if(referenceCounterparty.Subject == null)
			{
				referenceDeliveryPoint.Subject = null;
				referenceDeliveryPoint.Sensitive = false;
			}
			else
			{
				referenceDeliveryPoint.Subject = null;
				referenceDeliveryPoint.Sensitive = true;
				referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, 
					referenceCounterparty.GetSubject<Counterparty>());
			}
		}
	}
}


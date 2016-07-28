using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Client;

namespace Vodovoz.Reports
{
	public partial class RevisionBottlesAndDeposits : Gtk.Bin, IParametersWidget
	{
		IUnitOfWork uow;

		public RevisionBottlesAndDeposits()
		{
			this.Build();
			uow = UnitOfWorkFactory.CreateWithoutRoot ();
			referenceCounterparty.RepresentationModel = new ViewModel.CounterpartyVM(uow);
		}	

		#region IParametersWidget implementation

		public string Title
		{
			get
			{
				return "Акт по бутылям/залогам";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
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
					{ "startDate", dateperiodpicker1.StartDateOrNull.Value },
					{ "endDate", dateperiodpicker1.EndDateOrNull.Value },
					{ "client_id", referenceCounterparty.GetSubject<Counterparty>().Id},
					{ "delivery_point_id", referenceDeliveryPoint.Subject == null ? -1 : referenceDeliveryPoint.GetSubject<DeliveryPoint>().Id},
				}
			};
		}			

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			var datePeriodSelected = dateperiodpicker1.EndDateOrNull != null && dateperiodpicker1.StartDateOrNull != null;
			var counterpartySelected = referenceCounterparty.Subject != null;
			buttonRun.Sensitive = datePeriodSelected && counterpartySelected;
		}

		protected void OnReferenceCounterpartyChanged (object sender, EventArgs e)
		{
			ValidateParameters();
			if(referenceCounterparty.Subject == null)
			{
				referenceDeliveryPoint.Subject = null;
				referenceDeliveryPoint.Sensitive = false;
			}
			else
			{
				referenceDeliveryPoint.Sensitive = true;
				referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(uow, 
					referenceCounterparty.GetSubject<Counterparty>());
			}
		}
	}
}


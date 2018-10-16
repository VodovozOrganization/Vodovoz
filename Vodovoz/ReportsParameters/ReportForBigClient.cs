using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Client;

namespace Vodovoz.ReportsParameters
{
	public partial class ReportForBigClient : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public IUnitOfWork UoW { get; private set; }

		public ReportForBigClient()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			referenceCounterparty.RepresentationModel = new ViewModel.CounterpartyVM(UoW);
		}

		#region IParametersWidget implementation

		public string Title {
			get {
				return "Отчёт \"Куньголово\"";
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		public object EntityObject {
			get {
				return null;
			}
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Client.SummaryBottlesAndDepositsKungolovo",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", dateperiodpicker1.StartDateOrNull },
					{ "endDate", dateperiodpicker1.EndDateOrNull },
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
			var counterpartySelected = referenceCounterparty.Subject != null;
			buttonRun.Sensitive = counterpartySelected;
		}

		protected void OnReferenceCounterpartyChanged(object sender, EventArgs e)
		{
			ValidateParameters();
			if(referenceCounterparty.Subject == null) {
				referenceDeliveryPoint.Subject = null;
				referenceDeliveryPoint.Sensitive = false;
			} else {
				referenceDeliveryPoint.Subject = null;
				referenceDeliveryPoint.Sensitive = true;
				referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW,
					referenceCounterparty.GetSubject<Counterparty>());
			}
		}
	}
}

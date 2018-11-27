using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters.Payments
{
	public partial class PaymentsFromTinkoffReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public IUnitOfWork UoW { get; private set; }
		DateTime? startDate;
		public PaymentsFromTinkoffReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			startDate = pkrStartDate.Date = DateTime.Today.AddDays(-1);
			rbtnYesterday.Active = true;
			SetControlsAccessibility();
			rbtnLast3Days.Clicked += OnRbtnLast3DaysToggled;
			rbtnYesterday.Clicked += OnRbtnYesterdayToggled;
			rbtnCustomPeriod.Clicked += OnCustomPeriodChanged;
			pkrStartDate.DateChangedByUser += OnCustomPeriodChanged;
		}

		void SetControlsAccessibility()
		{
			pkrStartDate.IsEditable = pkrStartDate.Sensitive = rbtnCustomPeriod.Active;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по оплатам OnLine заказов";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null)
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Payments.PaymentsFromTinkoffReport",
				Parameters = new Dictionary<string, object> {
					{ "date", startDate }
				}
			};
		}

		protected void OnRbtnLast3DaysToggled(object sender, EventArgs e)
		{
			if(rbtnLast3Days.Active)
				startDate = DateTime.Today.AddDays(-3);
			SetControlsAccessibility();
		}

		protected void OnRbtnYesterdayToggled(object sender, EventArgs e)
		{
			if(rbtnYesterday.Active)
				startDate = DateTime.Today.AddDays(-1);
			SetControlsAccessibility();
		}

		protected void OnCustomPeriodChanged(object sender, EventArgs e)
		{
			if(rbtnCustomPeriod.Active)
				startDate = pkrStartDate.DateOrNull;
			SetControlsAccessibility();
		}
	}
}

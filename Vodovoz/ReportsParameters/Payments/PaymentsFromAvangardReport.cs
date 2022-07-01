using System;
using QSReport;
using QS.Report;
using System.Collections.Generic;

namespace Vodovoz.ReportsParameters.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsFromAvangardReport : Gtk.Bin, IParametersWidget
	{
		public PaymentsFromAvangardReport()
		{
			Build();
			Configure();
		}

		void Configure()
		{
			SetControlsAccessibility();
			rbtnLast3Days.Clicked += OnRbtnLast3DaysToggled;
			rbtnYesterday.Clicked += OnRbtnYesterdayToggled;
			rbtnCustomPeriod.Clicked += OnCustomPeriodChanged;
			dateperiodpicker.PeriodChangedByUser += OnCustomPeriodChanged;
			buttonRun.Clicked += OnButtonRunClicked;
			rbtnYesterday.Active = true;
		}

		void SetControlsAccessibility()
		{
			dateperiodpicker.Sensitive = rbtnCustomPeriod.Active;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по оплатам Авангарда";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private void OnButtonRunClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}

		private ReportInfo GetReportInfo()
		{
			var info = new ReportInfo
			{
				Identifier = "Payments.PaymentsFromAvangardReport",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", dateperiodpicker.StartDate },
					{ "endDate", dateperiodpicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
				}
			};
			return info;
		}

		private void OnRbtnLast3DaysToggled(object sender, EventArgs e)
		{
			if(rbtnLast3Days.Active)
			{
				dateperiodpicker.StartDate = DateTime.Today.AddDays(-3);
				dateperiodpicker.EndDate = DateTime.Today;
			}

			SetControlsAccessibility();
		}

		private void OnRbtnYesterdayToggled(object sender, EventArgs e)
		{
			if(rbtnYesterday.Active)
			{
				dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today.AddDays(-1);
			}
			SetControlsAccessibility();
		}

		private void OnCustomPeriodChanged(object sender, EventArgs e)
		{
			SetControlsAccessibility();
		}
	}
}

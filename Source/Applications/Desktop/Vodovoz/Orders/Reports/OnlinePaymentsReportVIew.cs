using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.EntityRepositories.Payments;
using QS.Project.Services;
using System.ComponentModel;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Orders.Reports;

namespace Vodovoz.Orders.Reports
{
	[ToolboxItem(true)]
	public partial class OnlinePaymentsReportView : DialogViewBase<OnlinePaymentsReportViewModel>
	{
		public OnlinePaymentsReportView(OnlinePaymentsReportViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			daterangepicker.StartDate = DateTime.Today.AddDays(-1);
			daterangepicker.EndDate = DateTime.Today;
            rbtnYesterday.Active = true;
			SetControlsAccessibility();
			rbtnLast3Days.Clicked += OnRbtnLast3DaysToggled;
			rbtnYesterday.Clicked += OnRbtnYesterdayToggled;
			rbtnCustomPeriod.Clicked += OnCustomPeriodChanged;
			daterangepicker.PeriodChangedByUser += OnCustomPeriodChanged;
			//ySCmbShop.SetRenderTextFunc<string>(o => string.IsNullOrWhiteSpace(o) ? "{ нет названия }" : o);
			//ySCmbShop.ItemsList = ViewModel.Shops;
		}

		private void SetControlsAccessibility()
		{
			daterangepicker.Sensitive = rbtnCustomPeriod.Active;
		}

		private ReportInfo GetReportInfo()
		{
			var rInfo = new ReportInfo {
				Identifier = "Payments.PaymentsFromTinkoffReport",
				Parameters = new Dictionary<string, object> {
					{ "startDate", daterangepicker.StartDate },
                    { "endDate", daterangepicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
                    //{ "shop", ySCmbShop.SelectedItem ?? "ALL" }
				}
			};
			return rInfo;
		}

		protected void OnRbtnLast3DaysToggled(object sender, EventArgs e)
		{
            if (rbtnLast3Days.Active)
            {
				daterangepicker.StartDate = DateTime.Today.AddDays(-3);
				daterangepicker.EndDate = DateTime.Today;
            }

            SetControlsAccessibility();
		}

		protected void OnRbtnYesterdayToggled(object sender, EventArgs e)
		{
			if(rbtnYesterday.Active)
            {
				daterangepicker.StartDate = DateTime.Today.AddDays(-1);
				daterangepicker.EndDate = DateTime.Today;
            }
            SetControlsAccessibility();
		}

		protected void OnCustomPeriodChanged(object sender, EventArgs e)
		{
			SetControlsAccessibility();
		}
	}
}

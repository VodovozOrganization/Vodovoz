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
			daterangepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangePeriodManually, w => w.Sensitive)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();


			rbtnLast3Days.Clicked += OnRbtnLast3DaysToggled;
			rbtnYesterday.Clicked += OnRbtnYesterdayToggled;

			speciallistcomboboxShop.SetRenderTextFunc<string>(o =>
				string.IsNullOrWhiteSpace(o) ? "{ нет названия }" : o);

			speciallistcomboboxShop.ItemsList = ViewModel.Shops;

			ViewModel.SetDateTimeRangeYesterdayCommand.Execute();
		}


		private ReportInfo GetReportInfo()
		{
			var rInfo = new ReportInfo
			{
				Identifier = "Payments.PaymentsFromTinkoffReport",
				Parameters = new Dictionary<string, object> {
					{ "startDate", daterangepicker.StartDate },
					{ "endDate", daterangepicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
					{ "shop", speciallistcomboboxShop.SelectedItem ?? "ALL" }
				}
			};
			return rInfo;
		}

		protected void OnRbtnLast3DaysToggled(object sender, EventArgs e)
		{
			if(rbtnLast3Days.Active)
			{
				ViewModel.SetDateTimeRangeLast3DaysCommand.Execute();
			}
		}

		protected void OnRbtnYesterdayToggled(object sender, EventArgs e)
		{
			if(rbtnYesterday.Active)
			{
				ViewModel.SetDateTimeRangeYesterdayCommand.Execute();
			}
		}
	}
}

using System;
using QS.Views.Dialog;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	public partial class PaymentsDiscrepanciesAnalysisView : DialogViewBase<PaymentsDiscrepanciesAnalysisViewModel>
	{
		public PaymentsDiscrepanciesAnalysisView(PaymentsDiscrepanciesAnalysisViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{

		}
	}
}

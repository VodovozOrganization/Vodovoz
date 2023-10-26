using QS.Views;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.ReportsParameters.Payments;

namespace Vodovoz.ReportsParameters.Payments
{
	[ToolboxItem(true)]
	public partial class PaymentsFromBankClientReportView : ViewBase<PaymentsFromBankClientReportViewModel>
	{
		public PaymentsFromBankClientReportView(PaymentsFromBankClientReportViewModel viewModel)
			: base(viewModel)
		{ 
			Build();

			btnCreateReport.Clicked += CreateReportClicked;

		    entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);

			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.Counterparty, w => w.Subject)
				.InitializeFromSource();

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;
		}

		private void CreateReportClicked(object sender, EventArgs e)
		{
			if(ViewModel.Validate())
			{
				ViewModel.LoadReport();
			}
		}
	}
}

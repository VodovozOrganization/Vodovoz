using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentFromBankClientView : TabViewBase<PaymentFromBankClientViewModel>
	{
		public PaymentFromBankClientView(PaymentFromBankClientViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}

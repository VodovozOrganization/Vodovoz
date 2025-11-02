using QS.Views.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Presentation.ViewModels.PaymentTypes;

namespace Vodovoz.Views
{
	public partial class SelectPaymentTypeWindowView : DialogViewBase<SelectPaymentTypeViewModel>
	{
		private const int _defaultWidth = 210;

		public SelectPaymentTypeWindowView(SelectPaymentTypeViewModel viewModel) : base(viewModel)
		{
			Build();

			WidthRequest = _defaultWidth;

			ybuttonCash.Clicked += (_, _2) => ViewModel.SelectPaymentTypeCommand.Execute(PaymentType.Cash);
			ybuttonCash.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentTypeCashVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonTerminal.Clicked += (_, _2) => ViewModel.SelectPaymentTypeCommand.Execute(PaymentType.Terminal);
			ybuttonTerminal.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentTypeTerminalVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonDriverApplication.Clicked += (_, _2) => ViewModel.SelectPaymentTypeCommand.Execute(PaymentType.DriverApplicationQR);
			ybuttonDriverApplication.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentTypeDriverApplicationVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonSmsQr.Clicked += (_, _2) => ViewModel.SelectPaymentTypeCommand.Execute(PaymentType.SmsQR);
			ybuttonSmsQr.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentTypeSmsQrVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonPaidOnline.Clicked += (_, _2) => ViewModel.SelectPaymentTypeCommand.Execute(PaymentType.PaidOnline);
			ybuttonPaidOnline.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentTypePaidOnlineVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonBarter.Clicked += (_, _2) => ViewModel.SelectPaymentTypeCommand.Execute(PaymentType.Barter);
			ybuttonBarter.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentTypeBarterVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonContractDocumentation.Clicked += (_, _2) => ViewModel.SelectPaymentTypeCommand.Execute(PaymentType.ContractDocumentation);
			ybuttonContractDocumentation.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentTypeContractDocumentationVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonCashless.Clicked += (_, _2) => ViewModel.SelectPaymentTypeCommand.Execute(PaymentType.Cashless);
			ybuttonCashless.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentTypeCashlessVisible, w => w.Visible)
				.InitializeFromSource();
		}
	}
}

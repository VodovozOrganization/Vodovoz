using QS.Views.GtkUI;
using QSWidgetLib;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
	public partial class PaymentByCardView : TabViewBase<PaymentByCardViewModel>
	{
		public PaymentByCardView(PaymentByCardViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonSave.BindCommand(ViewModel.SaveCommand);
			ybuttonCancel.BindCommand(ViewModel.CloseCommand);

			entryOnlineOrder.ValidationMode = (QS.Widgets.ValidationType)ValidationType.numeric;
			entryOnlineOrder.Binding.AddBinding(ViewModel.Entity,
				e => e.OnlinePaymentNumber,
				w => w.Text,
				new NullableIntToStringConverter()).InitializeFromSource();

			enumPaymentType.ItemsEnum = typeof(PaymentType);
			enumPaymentType.Binding.AddBinding(ViewModel, s => s.PaymentType, w => w.SelectedItem).InitializeFromSource();
			enumPaymentType.AddEnumToHideList(
				PaymentType.Barter,
				PaymentType.PaidOnline,
				PaymentType.Cash,
				PaymentType.Cashless,
				PaymentType.ContractDocumentation,
				PaymentType.DriverApplicationQR,
				PaymentType.SmsQR);
			enumPaymentType.SelectedItem = ViewModel.PaymentType;

			yenumcomboboxTerminalSubtype.ItemsEnum = typeof(PaymentByTerminalSource);
			yenumcomboboxTerminalSubtype.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(s => s.PaymentByTerminalSource, w => w.SelectedItem)
				.AddFuncBinding(s => s.PaymentType == PaymentType.Terminal, w => w.Visible)
				.InitializeFromSource();
		}
	}
}

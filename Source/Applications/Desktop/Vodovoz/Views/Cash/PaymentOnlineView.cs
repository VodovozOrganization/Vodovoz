using QS.Views.GtkUI;
using QSWidgetLib;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
	public partial class PaymentOnlineView : TabViewBase<PaymentOnlineViewModel>
	{
		public PaymentOnlineView(PaymentOnlineViewModel viewModel) : base(viewModel)
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

			comboPaymentFrom.SetRenderTextFunc<PaymentFrom>(x => x.Name);

			comboPaymentFrom.Binding.AddBinding(ViewModel,
				vm => vm.ItemsList, w => w.ItemsList)
				.InitializeFromSource();

			comboPaymentFrom.Binding.AddBinding(ViewModel,
				vm => vm.PaymentOnlineFrom, w => w.SelectedItem)
				.InitializeFromSource();
		}
	}
}


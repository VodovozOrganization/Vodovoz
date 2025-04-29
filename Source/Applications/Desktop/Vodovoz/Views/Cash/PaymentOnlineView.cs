using QS.Navigation;
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
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonSave.Binding.AddFuncBinding(ViewModel.Entity,
				e => e.OnlinePaymentNumber.HasValue,
				w => w.Sensitive).InitializeFromSource();

			ybuttonSave.Clicked += (s, ea) => ViewModel.SaveAndClose();
			ybuttonCancel.Clicked += (s, ea) => ViewModel.Close(true, CloseSource.Cancel);

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


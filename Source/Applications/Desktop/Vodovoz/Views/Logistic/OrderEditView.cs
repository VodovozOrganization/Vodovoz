using QS.Views.GtkUI;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public class OrderEditView : TabViewBase<OrderEditViewModel>
	{
		public OrderEditView(OrderEditViewModel viewModel) : base(viewModel)
		{
			//Build();
			Configure();
		}

		private void Configure()
		{
			/*ybuttonSave.BindCommand(ViewModel.SaveCommand);
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
				.InitializeFromSource();*/
		}
	}
}

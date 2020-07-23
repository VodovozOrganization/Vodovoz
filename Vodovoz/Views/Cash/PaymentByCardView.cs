using System.Linq;
using QS.Views.GtkUI;
using QSWidgetLib;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
	public partial class PaymentByCardView : TabViewBase<PaymentByCardViewModel>
	{
		public PaymentByCardView(PaymentByCardViewModel viewModel) : base(viewModel) {
			this.Build();
			Configure();
		}

		private void Configure() {
			ybuttonSave.Binding.AddFuncBinding(ViewModel.Entity, e => e.OnlineOrder.HasValue, w => w.Sensitive).InitializeFromSource();
			ybuttonSave.Clicked += (s, ea) => ViewModel.SaveAndClose();
			
			entryOnlineOrder.ValidationMode = ValidationType.numeric;
			entryOnlineOrder.Binding.AddBinding(ViewModel.Entity, e => e.OnlineOrder, w => w.Text, new IntToStringConverter()).InitializeFromSource();
			
			comboPaymentFrom.SetRenderTextFunc<PaymentFrom>(x => x.Name);
			comboPaymentFrom.Binding.AddBinding(ViewModel.Entity, vm => vm.PaymentByCardFrom, w => w.SelectedItem).InitializeFromSource();
			comboPaymentFrom.ItemsList = ViewModel.UoW.GetAll<PaymentFrom>();
		}
	}
}

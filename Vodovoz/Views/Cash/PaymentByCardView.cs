using System;
using System.ServiceModel.Channels;
using GMap.NET.MapProviders;
using QS.Dialog.GtkUI;
using QS.Views.GtkUI;
using Vodovoz.Domain.Orders;
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
			int tryParseTemp;
			// ybuttonSave.Binding.AddFuncBinding(ViewModel, vm => vm.StringText.Length != 0, w => w.Sensitive).InitializeFromSource();
			ybuttonSave.Binding.AddFuncBinding(ViewModel, vm => int.TryParse(vm.OnlineOrderNumber, out tryParseTemp), w => w.Sensitive).InitializeFromSource();
			ybuttonSave.Clicked += (s, ea) => ViewModel.SaveItemCommand.Execute();

			yentry2.Binding.AddBinding(ViewModel, vm => vm.OnlineOrderNumber, w => w.Text).InitializeFromSource();
			
			ylistcombobox1.SetRenderTextFunc<PaymentFrom>(x => x.Name);
			ylistcombobox1.Binding.AddBinding(ViewModel, vm => vm.PaymentsFrom, e => e.ItemsList).InitializeFromSource();
			ylistcombobox1.Binding.AddBinding(ViewModel, vm => vm.SelectedPaymentFrom, w => w.SelectedItem).InitializeFromSource();
		}
	}
}

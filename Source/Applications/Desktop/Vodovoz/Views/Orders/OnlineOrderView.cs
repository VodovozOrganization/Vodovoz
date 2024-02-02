using System;
using QS.Views.GtkUI;
using QS.Navigation;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class OnlineOrderView : TabViewBase<OnlineOrderViewModel>
	{
		public OnlineOrderView(OnlineOrderViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}

using QS.Views;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OnlineOrderTemplateView : TabViewBase<OnlineOrderTemplateViewModel>
	{
		public OnlineOrderTemplateView(OnlineOrderTemplateViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			
		}
	}
}

using QS.Views;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OnlineOrderTemplateView : ViewBase<OnlineOrderTemplateViewModel>
	{
		public OnlineOrderTemplateView(OnlineOrderTemplateViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}

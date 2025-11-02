using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersWithMinPriceLessThan : ViewBase<OrdersWithMinPriceLessThanViewModel>
	{
		public OrdersWithMinPriceLessThan(OrdersWithMinPriceLessThanViewModel viewModel) : base(viewModel)
		{
			this.Build();
			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}

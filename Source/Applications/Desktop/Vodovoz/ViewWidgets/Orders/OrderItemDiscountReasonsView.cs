using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Widgets.Orders;
namespace Vodovoz.ViewWidgets.Orders
{
	[ToolboxItem(true)]
	public partial class OrderItemDiscountReasonsView : WidgetViewBase<OrderItemDiscountReasonsViewModel>
	{
		public OrderItemDiscountReasonsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();
		}
	}
}

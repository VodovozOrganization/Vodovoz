using System;
using QS.ViewModels;
using QS.Views.GtkUI;

namespace Vodovoz.ViewWidgets.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderDateFilterView : WidgetViewBase<OrderDateFilterViewModel>
	{
		public OrderDateFilterView()
		{
			this.Build();
		}
	}

	public partial class OrderDateFilterViewModel : WidgetViewModelBase
	{

	}
}

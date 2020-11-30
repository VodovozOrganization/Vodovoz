using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FixedPricesView : WidgetViewBase<FixedPricesViewModel>
	{
		public FixedPricesView(FixedPricesViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{

		}
	}
}

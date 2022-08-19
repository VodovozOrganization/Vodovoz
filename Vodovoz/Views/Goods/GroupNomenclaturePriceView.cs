using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Dialogs.Goods;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GroupNomenclaturePriceView : TabViewBase<NomenclatureGroupPricingViewModel>
	{
		public GroupNomenclaturePriceView(NomenclatureGroupPricingViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{

		}
	}
}

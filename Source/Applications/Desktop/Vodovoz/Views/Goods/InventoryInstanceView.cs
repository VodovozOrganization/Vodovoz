using System;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class InventoryInstanceView : ViewBase<InventoryInstanceViewModel>
	{
		public InventoryInstanceView(InventoryInstanceViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			throw new NotImplementedException();
		}
	}
}

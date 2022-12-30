using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.JournalFilters.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InventoryInstancesJournalFilterView : FilterViewBase<InventoryInstancesJournalFilterViewModel>
	{
		public InventoryInstancesJournalFilterView(InventoryInstancesJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{

		}
	}
}

using System;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Warehouses;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WarehouseFilterView : FilterViewBase<WarehouseJournalFilterViewModel>
	{
		public WarehouseFilterView(WarehouseJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ycheckRestrictArchive.Binding.AddBinding(ViewModel, vm => vm.ResctrictArchive, w => w.Active).InitializeFromSource();
		}
	}
}

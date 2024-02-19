using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WarehouseJournalFilterView : FilterViewBase<WarehouseJournalFilterViewModel>
	{
		public WarehouseJournalFilterView(WarehouseJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}

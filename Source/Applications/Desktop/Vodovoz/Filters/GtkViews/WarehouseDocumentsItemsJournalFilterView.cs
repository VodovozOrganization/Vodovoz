using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WarehouseDocumentsItemsJournalFilterView : FilterViewBase<WarehouseDocumentsItemsJournalFilterViewModel>
	{
		public WarehouseDocumentsItemsJournalFilterView(WarehouseDocumentsItemsJournalFilterViewModel filterViewModel)
			: base(filterViewModel)
		{
			Build();
		}
	}
}

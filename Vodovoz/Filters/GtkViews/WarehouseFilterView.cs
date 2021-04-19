using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class WarehouseFilterView : FilterViewBase<WarehouseJournalFilterViewModel>
    {
        public WarehouseFilterView(WarehouseJournalFilterViewModel filterViewModel) : base(filterViewModel)
        {
            this.Build();
        }
    }
}

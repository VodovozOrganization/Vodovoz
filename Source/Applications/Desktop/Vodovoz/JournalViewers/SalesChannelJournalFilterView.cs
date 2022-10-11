using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Retail;

namespace Vodovoz.JournalViewers
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class SalesChannelJournalFilterView : FilterViewBase<SalesChannelJournalFilterViewModel>
    {
        public SalesChannelJournalFilterView(SalesChannelJournalFilterViewModel viewModel) : base(viewModel)
        {
            this.Build();
        }
    }
}

using System;
namespace Vodovoz.JournalViewers.Retail
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class SalesChannelJournalFilterView : FilterViewBase<SalesChannelJournalFilterViewModel>
    {
        public SalesChannelJournalFilterView(SalesChannelJournalFilterViewModel viewModel) : base(SalesChannelJournalFilterViewModel)
        {
            this.Build();
        }
    }
}

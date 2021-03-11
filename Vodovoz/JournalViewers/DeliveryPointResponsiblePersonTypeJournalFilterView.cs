using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.JournalViewers
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class DeliveryPointResponsiblePersonTypeJournalFilterView : FilterViewBase<DeliveryPointResponsiblePersonTypeJournalFilterViewModel>
    {
        public DeliveryPointResponsiblePersonTypeJournalFilterView(DeliveryPointResponsiblePersonTypeJournalFilterViewModel journalFilterViewModel) : base(journalFilterViewModel)
        {
            this.Build();
        }
    }
}

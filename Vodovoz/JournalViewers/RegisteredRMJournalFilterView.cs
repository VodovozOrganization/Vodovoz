using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;

namespace Vodovoz.JournalViewers
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class RegisteredRMJournalFilterView : FilterViewBase<RegisteredRMJournalFilterViewModel>
    {
        public RegisteredRMJournalFilterView(RegisteredRMJournalFilterViewModel viewModel) : base (viewModel)
        {
            this.Build();
        }
    }
}

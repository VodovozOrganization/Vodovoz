using QS.Views.GtkUI;
using Vodovoz.Domain.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;

namespace Vodovoz.JournalFilters.Proposal
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class ApplicationDevelopmentProposalsJournalFilterView : 
        FilterViewBase<ApplicationDevelopmentProposalsJournalFilterViewModel>
    {
        public ApplicationDevelopmentProposalsJournalFilterView(
            ApplicationDevelopmentProposalsJournalFilterViewModel viewModel) : base(viewModel)
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            yEnumCmbStatus.ItemsEnum = typeof(ApplicationDevelopmentProposalStatus);
            yEnumCmbStatus.Binding.AddBinding(ViewModel, vm => vm.Status, w => w.SelectedItemOrNull).InitializeFromSource();
        }
    }
}

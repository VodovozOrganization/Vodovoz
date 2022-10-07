using QS.Project.Filter;
using Vodovoz.Domain.Proposal;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Proposal
{
    public class ApplicationDevelopmentProposalsJournalFilterViewModel : 
        FilterViewModelBase<ApplicationDevelopmentProposalsJournalFilterViewModel>
    {
        ApplicationDevelopmentProposalStatus? status;
        public virtual ApplicationDevelopmentProposalStatus? Status {
            get => status;
            set => UpdateFilterField(ref status, value);
        }
    }
}
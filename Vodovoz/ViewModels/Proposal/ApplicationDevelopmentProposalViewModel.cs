using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Proposal;

namespace Vodovoz.ViewModels.Proposal
{
    public class ApplicationDevelopmentProposalViewModel : EntityTabViewModelBase<ApplicationDevelopmentProposal>
    {
        public ApplicationDevelopmentProposalViewModel(
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory uowFactory,
            ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
        {
                
        }
    }
}
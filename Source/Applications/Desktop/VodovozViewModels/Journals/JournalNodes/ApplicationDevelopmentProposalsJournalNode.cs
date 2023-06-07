using System;
using QS.Project.Journal;
using Vodovoz.Domain.Proposal;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class ApplicationDevelopmentProposalsJournalNode : JournalEntityNodeBase<ApplicationDevelopmentProposal>
    {
		public override string Title => Subject;

		public string Subject { get; set; }
		public DateTime CreationDate { get; set; }
        public ApplicationDevelopmentProposalStatus Status { get; set; }
    }
}

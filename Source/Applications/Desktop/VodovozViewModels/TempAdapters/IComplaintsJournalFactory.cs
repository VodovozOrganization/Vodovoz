using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IComplaintsJournalFactory
	{
		ComplaintsWithDepartmentsReactionJournalViewModel GetJournalWithDepartmentsReaction(ComplaintFilterViewModel filterViewModel);
		ComplaintsJournalViewModel GetStandartJournal(ComplaintFilterViewModel filterViewModel);
	}
}

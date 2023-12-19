using Autofac;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface IComplaintKindJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateComplaintKindAutocompleteSelectorFactory(
			ILifetimeScope lifetimeScope, ComplaintKindJournalFilterViewModel filterViewModel = null);
	}
}

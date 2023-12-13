using Autofac;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.TempAdapters
{
	public class ComplaintKindJournalFactory : IComplaintKindJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateComplaintKindAutocompleteSelectorFactory(
			ILifetimeScope lifetimeScope, ComplaintKindJournalFilterViewModel filterViewModel = null)
		{
			return new EntityAutocompleteSelectorFactory<ComplaintKindJournalViewModel>(typeof(ComplaintKind),
				() =>
				{
					ComplaintKindJournalViewModel journalViewModel = null;

					if(filterViewModel is null)
					{
						journalViewModel = lifetimeScope.Resolve<ComplaintKindJournalViewModel>();
					}
					else
					{
						journalViewModel = lifetimeScope.Resolve<ComplaintKindJournalViewModel>(
							new TypedParameter(typeof(ComplaintKindJournalFilterViewModel), filterViewModel));
					}

					return journalViewModel;
				});
		}
	}
}

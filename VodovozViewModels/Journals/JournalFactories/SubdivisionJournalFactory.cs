using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class SubdivisionJournalFactory : ISubdivisionJournalFactory
	{
		private readonly IJournalFilter _subdivisionJournalFilter;

		public SubdivisionJournalFactory(IJournalFilter subdivisionJournalFilter = null)
		{
			_subdivisionJournalFilter = subdivisionJournalFilter;
		}
		public IEntityAutocompleteSelectorFactory CreateSubdivisionAutocompleteSelectorFactory(IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(typeof(Subdivision), () =>
			{
				return new SubdivisionsJournalViewModel((_subdivisionJournalFilter as SubdivisionFilterViewModel) ?? new SubdivisionFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, employeeSelectorFactory);
			});
		}
	}
}

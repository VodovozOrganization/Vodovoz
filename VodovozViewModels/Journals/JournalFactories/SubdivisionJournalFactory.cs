using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class SubdivisionJournalFactory : ISubdivisionJournalFactory
	{
		private readonly SubdivisionFilterViewModel _subdivisionJournalFilter;

		public SubdivisionJournalFactory(SubdivisionFilterViewModel subdivisionJournalFilter = null)
		{
			_subdivisionJournalFilter = subdivisionJournalFilter;
		}
		public IEntityAutocompleteSelectorFactory CreateSubdivisionAutocompleteSelectorFactory(IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			ISalesPlanJournalFactory salesPlanJournalFactory, INomenclatureSelectorFactory nomenclatureSelectorFactory)
		{
			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(typeof(Subdivision), () =>
			{
				return new SubdivisionsJournalViewModel(_subdivisionJournalFilter ?? new SubdivisionFilterViewModel(), UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices, employeeSelectorFactory, salesPlanJournalFactory, nomenclatureSelectorFactory);
			});
		}
	}
}

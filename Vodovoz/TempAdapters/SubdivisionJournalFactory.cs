using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.TempAdapters
{
	public class SubdivisionJournalFactory : ISubdivisionJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => new SubdivisionsJournalViewModel(
					new SubdivisionFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					employeeSelectorFactory,
					new SalesPlanJournalFactory(),
					new NomenclatureSelectorFactory()));
		}
		
		public IEntityAutocompleteSelectorFactory CreateDefaultSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => new SubdivisionsJournalViewModel(
					new SubdivisionFilterViewModel
					{
						SubdivisionType = SubdivisionType.Default
					},
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					employeeSelectorFactory,
					new SalesPlanJournalFactory(),
					new NomenclatureSelectorFactory()));
		}

		public IEntityAutocompleteSelectorFactory CreateLogisticSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory)
		{
			return new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
				typeof(Subdivision),
				() => new SubdivisionsJournalViewModel(
					new SubdivisionFilterViewModel
					{
						SubdivisionType = SubdivisionType.Logistic
					},
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					employeeSelectorFactory,
					new SalesPlanJournalFactory(),
					new NomenclatureSelectorFactory()));
		}
	}
}

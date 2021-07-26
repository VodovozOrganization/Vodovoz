using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalFactories
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
					employeeSelectorFactory));
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
					employeeSelectorFactory));
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
					employeeSelectorFactory));
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

using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class SalesPlanJournalFactory : ISalesPlanJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSalesPlanAutocompleteSelectorFactory(INomenclatureSelectorFactory nomenclatureSelectorFactory)
		{
			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
			
			return new EntityAutocompleteSelectorFactory<SalesPlanJournalViewModel>(
				typeof(SalesPlan),
				() => new SalesPlanJournalViewModel(
					journalActions, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices,  nomenclatureSelectorFactory));
		}
	}
}

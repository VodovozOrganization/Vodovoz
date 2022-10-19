using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class SalesPlanJournalFactory : ISalesPlanJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSalesPlanAutocompleteSelectorFactory(INomenclatureJournalFactory nomenclatureSelectorFactory)
		{
			return new EntityAutocompleteSelectorFactory<SalesPlanJournalViewModel>(typeof(SalesPlan), () =>
			{
				return new SalesPlanJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices,  nomenclatureSelectorFactory);
			});
		}
	}
}

using QS.DomainModel.UoW;
using QS.Navigation;
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
		public IEntityAutocompleteSelectorFactory CreateSalesPlanAutocompleteSelectorFactory(INavigationManager navigationManager)
		{
			return new EntityAutocompleteSelectorFactory<SalesPlanJournalViewModel>(
				typeof(SalesPlan),
				() => new SalesPlanJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, navigationManager));
		}
	}
}

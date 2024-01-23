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
		private readonly IUnitOfWorkFactory _uowFactory;

		public SalesPlanJournalFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateSalesPlanAutocompleteSelectorFactory(INavigationManager navigationManager)
		{
			return new EntityAutocompleteSelectorFactory<SalesPlanJournalViewModel>(
				typeof(SalesPlan),
				() => new SalesPlanJournalViewModel(_uowFactory, ServicesConfig.CommonServices, navigationManager));
		}
	}
}

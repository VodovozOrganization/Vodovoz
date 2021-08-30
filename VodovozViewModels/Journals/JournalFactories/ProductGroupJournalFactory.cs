using QS.DomainModel.UoW;
using QS.Project.Journal.Actions.ViewModels;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class ProductGroupJournalFactory : IProductGroupJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateProductGroupAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<ProductGroupJournalViewModel>(typeof(ProductGroup), () =>
			{
				var journalActionsViewModel = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
				
				return new ProductGroupJournalViewModel(
					journalActionsViewModel,
					new ProductGroupJournalFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					new ProductGroupJournalFactory());
			});
		}
	}
}

using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class ProductGroupJournalFactory : IProductGroupJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateProductGroupAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<ProductGroupJournalViewModel>(typeof(ProductGroup), () =>
			{
				return new ProductGroupJournalViewModel(new ProductGroupJournalFilterViewModel(), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, new ProductGroupJournalFactory());
			});
		}
	}
}

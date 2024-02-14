using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class ProductGroupJournalFactory : IProductGroupJournalFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public ProductGroupJournalFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public IEntityAutocompleteSelectorFactory CreateProductGroupAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<ProductGroupJournalViewModel>(
				typeof(ProductGroup),
				CreateProductGroupJournal);
		}
		
		public IEntityAutocompleteSelector CreateProductGroupAutocompleteSelector(bool multipleSelection = false)
		{
			var journal = CreateProductGroupJournal();
			journal.SelectionMode = multipleSelection ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
			return journal;
		}

		private ProductGroupJournalViewModel CreateProductGroupJournal() =>
			new ProductGroupJournalViewModel(
				new ProductGroupJournalFilterViewModel(),
				_uowFactory,
				ServicesConfig.CommonServices,
				this);
	}
}

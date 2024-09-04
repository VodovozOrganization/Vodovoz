using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Store
{
	public class WarehouseDocumentsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<WarehouseDocumentsJournalNode, WarehouseDocumentsJournalFilterViewModel>
	{
		public WarehouseDocumentsJournalViewModel(
			WarehouseDocumentsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices, navigationManager)
		{
		}
	}
}

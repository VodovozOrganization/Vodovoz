using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Store
{
	public class WarehouseDocumentsItemsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<WarehouseDocumentsItemsJournalNode, WarehouseDocumentsItemsJournalFilterViewModel>
	{
		public WarehouseDocumentsItemsJournalViewModel(
			WarehouseDocumentsItemsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал строк складских документов";
		}
	}
}

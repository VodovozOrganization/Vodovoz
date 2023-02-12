using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Documents;

namespace Vodovoz.ViewModels.ViewModels.Warehouses.Documents
{
	public class InventoryDocumentViewModel : EntityTabViewModelBase<InventoryDocument>
	{
		public InventoryDocumentViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null)
		 : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
		}
	}
}

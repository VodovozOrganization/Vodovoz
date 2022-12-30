using System;
using QS.ViewModels;
using Vodovoz.Domain.Documents;
using QS.Project.Domain;
using QS.Services;
using QS.Navigation;
using QS.DomainModel.UoW;
namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class InventoryInstanceViewModel : EntityTabViewModelBase<InventoryDocument>
	{
		public InventoryInstanceViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager) : base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{

		}
	}
}

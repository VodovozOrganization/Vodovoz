using System;
using Autofac;
using QS.ViewModels;
using QS.Project.Domain;
using QS.Services;
using QS.Navigation;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class InventoryInstanceViewModel : EntityTabViewModelBase<InventoryNomenclatureInstance>
	{
		public InventoryInstanceViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope) : base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}
		
		public ILifetimeScope Scope { get; }
	}
}

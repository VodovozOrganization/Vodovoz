using System;
using Autofac;
using QS.ViewModels;
using QS.Project.Domain;
using QS.Services;
using QS.Navigation;
using QS.DomainModel.UoW;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

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
			Initialize();
		}
		
		public InventoryInstanceViewModel(
			InventoryNomenclatureInstance inventoryNomenclatureInstance,
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope) : this(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager, scope)
		{
			CopyEntityWithoutInventoryNumber(inventoryNomenclatureInstance);
		}

		public ILifetimeScope Scope { get; }
		public IEntityEntryViewModel NomenclatureViewModel { get; private set; }

		private void Initialize()
		{
			var builder = new CommonEEVMBuilderFactory<InventoryNomenclatureInstance>(
				this, Entity, UoW, NavigationManager, Scope);

			NomenclatureViewModel = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelDialog<NomenclatureViewModel>()
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel>()
				.Finish();
		}
		
		private void CopyEntityWithoutInventoryNumber(InventoryNomenclatureInstance inventoryNomenclatureInstance)
		{
			Entity.Nomenclature = inventoryNomenclatureInstance.Nomenclature;
			Entity.CostPrice = inventoryNomenclatureInstance.CostPrice;
			Entity.PurchasePrice = inventoryNomenclatureInstance.PurchasePrice;
			Entity.InnerDeliveryPrice = inventoryNomenclatureInstance.InnerDeliveryPrice;
		}
	}
}

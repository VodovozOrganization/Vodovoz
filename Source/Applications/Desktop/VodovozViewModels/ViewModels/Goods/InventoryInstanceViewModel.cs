using System;
using Autofac;
using QS.ViewModels;
using QS.Project.Domain;
using QS.Services;
using QS.Navigation;
using QS.DomainModel.UoW;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class InventoryInstanceViewModel : EntityTabViewModelBase<InventoryNomenclatureInstance>, IAskSaveOnCloseViewModel
	{
		private readonly INomenclatureInstanceRepository _nomenclatureInstanceRepository;
		private bool _oldIsArchive;
		
		public InventoryInstanceViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			INomenclatureInstanceRepository nomenclatureInstanceRepository,
			ILifetimeScope scope) : base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_nomenclatureInstanceRepository =
				nomenclatureInstanceRepository ?? throw new ArgumentNullException(nameof(nomenclatureInstanceRepository));
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			Initialize();
		}
		
		public InventoryInstanceViewModel(
			Nomenclature nomenclature,
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			INomenclatureInstanceRepository nomenclatureInstanceRepository,
			ILifetimeScope scope)
			: this(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager, nomenclatureInstanceRepository, scope)
		{
			CopyEntityWithoutInventoryNumber(nomenclature);
		}

		public bool CanEdit { get; private set; }
		public bool CanEditNewEntity { get; private set; }
		public bool AskSaveOnClose => CanEdit;
		public ILifetimeScope Scope { get; }
		public IEntityEntryViewModel NomenclatureViewModel { get; private set; }

		protected override bool BeforeSave()
		{
			if(_oldIsArchive == Entity.IsArchive)
			{
				return true;
			}

			if(Entity.IsArchive)
			{
				if(_nomenclatureInstanceRepository.GetNomenclatureInstanceBalance(UoW, Entity.Id) > 0)
				{
					ShowWarningMessage("Нельзя заархивировать экземпляр, т.к. у него положительный баланс");
					Entity.IsArchive = false;
					return false;
				}
			}

			return true;
		}

		private void Initialize()
		{
			CanEdit = PermissionResult.CanUpdate || (Entity.Id == 0 && PermissionResult.CanCreate);
			CanEditNewEntity = Entity.Id == 0 && (PermissionResult.CanUpdate || PermissionResult.CanCreate);
			_oldIsArchive = Entity.IsArchive;
			
			var builder = new CommonEEVMBuilderFactory<InventoryNomenclatureInstance>(
				this, Entity, UoW, NavigationManager, Scope);

			NomenclatureViewModel = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelDialog<NomenclatureViewModel>()
				.UseViewModelJournalAndAutocompleter<InventoryNomenclaturesJournalViewModel>()
				.Finish();
		}
		
		private void CopyEntityWithoutInventoryNumber(Nomenclature nomenclature)
		{
			Entity.Nomenclature = nomenclature;
		}
	}
}

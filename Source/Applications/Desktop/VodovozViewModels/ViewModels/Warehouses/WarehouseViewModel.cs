using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Warehouses
{
	public class WarehouseViewModel : EntityTabViewModelBase<Warehouse>
	{
		private ILifetimeScope _lifetimeScope;

		public WarehouseViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionRepository,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			
			TabName = Entity?.Id == 0 ? "Новый склад" : Entity?.Name;
			Subdivisions = subdivisionRepository.GetAllDepartmentsOrderedByName(UoW);

			SetPermissions();
			ConfigureViewModels();
		}

		public bool CanEdit => PermissionResult.CanUpdate || (Entity.Id == 0 && PermissionResult.CanCreate);

		public bool CanArchiveWarehouse { get; private set; }
		public IList<Subdivision> Subdivisions { get; }
		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }

		private void SetPermissions()
		{
			CanArchiveWarehouse = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_archive_warehouse");
		}
		
		private void ConfigureViewModels()
		{
			SubdivisionViewModel = new CommonEEVMBuilderFactory<Warehouse>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(e => e.MovementDocumentsNotificationsSubdivisionRecipient)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}

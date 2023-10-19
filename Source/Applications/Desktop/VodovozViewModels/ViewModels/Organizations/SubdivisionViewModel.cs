using Autofac;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.ViewModels.ViewModels.Organizations
{
	public class SubdivisionViewModel : EntityTabViewModelBase<Subdivision>
	{
		private readonly ILifetimeScope _scope;
		private PresetSubdivisionPermissionsViewModel _presetSubdivisionPermissionVm;
		private WarehousePermissionsViewModel _warehousePermissionsVm;

		public SubdivisionViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IPermissionRepository permissionRepository,
			ISubdivisionRepository subdivisionRepository,
			INavigationManager navigationManager,
			ILifetimeScope scope) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			SubdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			PresetSubdivisionPermissionVM =
				_scope.Resolve<PresetSubdivisionPermissionsViewModel>(
					new TypedParameter(typeof(IUnitOfWork), UoW),
					new TypedParameter(typeof(Subdivision), Entity));
			var warehousePermissionModel = new SubdivisionWarehousePermissionModel(UoW, Entity);
			WarehousePermissionsVM = new WarehousePermissionsViewModel(UoW, warehousePermissionModel)
			{
				CanEdit = PermissionResult.CanUpdate
			};
			var permissionListViewModel = new PermissionListViewModel(PermissionExtensionSingletonStore.GetInstance());
			EntitySubdivisionPermissionViewModel = new EntitySubdivisionPermissionViewModel(
				UoW, Entity, permissionListViewModel, permissionRepository);

			ChiefViewModel = new CommonEEVMBuilderFactory<Subdivision>(this, Entity, UoW, NavigationManager, scope)
				.ForProperty(x => x.Chief)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournal<EmployeesJournalViewModel>()
				.Finish();

			ParentSubdivisionViewModel = new CommonEEVMBuilderFactory<Subdivision>(this, Entity, UoW, NavigationManager, scope)
				.ForProperty(x => x.ParentSubdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournal<SubdivisionsJournalViewModel>()
				.Finish();

			DefaultSalesPlanViewModel = new CommonEEVMBuilderFactory<Subdivision>(this, Entity, UoW, NavigationManager, scope)
				.ForProperty(x => x.DefaultSalesPlan)
				.UseViewModelDialog<SalesPlanViewModel>()
				.UseViewModelJournal<SalesChannelJournalViewModel>()
				.Finish();

			ConfigureEntityChangingRelations();
			CreateCommands();

			SubscribeUpdateOnChanges();
		}

		public ISubdivisionRepository SubdivisionRepository { get; }
		public IEntityEntryViewModel ChiefViewModel { get; private set; }
		public IEntityEntryViewModel ParentSubdivisionViewModel { get; private set; }
		public IEntityEntryViewModel DefaultSalesPlanViewModel { get; private set; }

		public EntitySubdivisionPermissionViewModel EntitySubdivisionPermissionViewModel { get; }

		public PresetSubdivisionPermissionsViewModel PresetSubdivisionPermissionVM
		{
			get => _presetSubdivisionPermissionVm;
			set => SetField(ref _presetSubdivisionPermissionVm, value);
		}

		public WarehousePermissionsViewModel WarehousePermissionsVM
		{
			get => _warehousePermissionsVm;
			set => SetField(ref _warehousePermissionsVm, value);
		}

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.GeographicGroup,
				() => GeographicGroup
			);

			SetPropertyChangeRelation(e => e.ParentSubdivision,
				() => GeographicGroupVisible
			);

			Entity.ObservableChildSubdivisions.ElementAdded += (aList, aIdx) => OnPropertyChanged(() => GeographicGroupVisible);
			Entity.ObservableChildSubdivisions.ElementRemoved += (aList, aIdx, aObject) => OnPropertyChanged(() => GeographicGroupVisible);
		}

		public bool CanEdit => PermissionResult.CanUpdate;

		public bool GeographicGroupVisible => Entity.ParentSubdivision != null && Entity.ChildSubdivisions.Any();

		public virtual GeoGroup GeographicGroup {
			get => Entity.GeographicGroup;
			set {
				if(Entity.GeographicGroup == value) {
					return;
				}
				Entity.GeographicGroup = value;
				Entity.SetChildsGeographicGroup(Entity.GeographicGroup);
			}
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAddDocumentTypeCommand();
			CreateDeleteDocumentCommand();
		}

		#region AddDocumentTypeCommand

		public DelegateCommand<TypeOfEntity> AddDocumentTypeCommand { get; private set; }

		private void CreateAddDocumentTypeCommand()
		{
			AddDocumentTypeCommand = new DelegateCommand<TypeOfEntity>(
				(docType) => Entity.AddDocumentType(docType),
				(docType) => docType != null && CanEdit
			);
		}

		#endregion AddDocumentTypeCommand

		#region DeleteDocumentCommand

		public DelegateCommand<TypeOfEntity> DeleteDocumentCommand { get; private set; }

		private void CreateDeleteDocumentCommand()
		{
			DeleteDocumentCommand = new DelegateCommand<TypeOfEntity>(
				(docType) => Entity.DeleteDocumentType(docType),
				(docType) => docType != null && CanEdit
			);
		}

		#endregion DeleteDocumentCommand

		#endregion Commands

		protected override bool BeforeSave()
		{
			EntitySubdivisionPermissionViewModel.SavePermissions();
			PresetSubdivisionPermissionVM.SaveCommand.Execute();
			WarehousePermissionsVM.SaveWarehousePermissions();
			return base.BeforeSave();
		}

		public override void Dispose()
		{
			UnsubscribeUpdateOnChanges();
			base.Dispose();
		}

		private void SubscribeUpdateOnChanges()
		{
			Entity.PropertyChanged += UpdateChanges;
			Entity.ObservableDocumentTypes.ListContentChanged += UpdateChanges;
			EntitySubdivisionPermissionViewModel.ObservableTypeOfEntitiesList.ListContentChanged += UpdateChanges;
			PresetSubdivisionPermissionVM.ObservablePermissionsList.ListContentChanged += UpdateChanges;
			
			foreach(var warehousePermissionNode in WarehousePermissionsVM.AllWarehouses)
			{
				warehousePermissionNode.SubNodeViewModel.ListContentChanged += UpdateChanges;
			}
		}
		
		private void UnsubscribeUpdateOnChanges()
		{
			Entity.PropertyChanged -= UpdateChanges;
			Entity.ObservableDocumentTypes.ListContentChanged -= UpdateChanges;
			EntitySubdivisionPermissionViewModel.ObservableTypeOfEntitiesList.ListContentChanged -= UpdateChanges;
			PresetSubdivisionPermissionVM.ObservablePermissionsList.ListContentChanged -= UpdateChanges;
			
			foreach(var warehousePermissionNode in WarehousePermissionsVM.AllWarehouses)
			{
				warehousePermissionNode.SubNodeViewModel.ListContentChanged -= UpdateChanges;
			}
		}

		private void UpdateChanges(object sender, EventArgs e) => HasChanges = true;
	}
}

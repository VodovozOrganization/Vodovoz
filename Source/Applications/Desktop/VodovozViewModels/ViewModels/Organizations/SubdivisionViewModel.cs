using Autofac;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.ViewModels.ViewModels.Organizations
{
	public class SubdivisionViewModel : EntityTabViewModelBase<Subdivision>
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ILifetimeScope _scope;
		private PresetSubdivisionPermissionsViewModel _presetSubdivisionPermissionVm;
		private WarehousePermissionsViewModel _warehousePermissionsVm;

		public SubdivisionViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			IPermissionRepository permissionRepository,
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ISubdivisionRepository subdivisionRepository,
			ILifetimeScope scope) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
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
			EmployeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			SalesPlanSelectorFactory = (salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory)))
				.CreateSalesPlanAutocompleteSelectorFactory(nomenclatureSelectorFactory);
			ConfigureEntityChangingRelations();
			CreateCommands();

			SubscribeUpdateOnChanges();
		}

		public ISubdivisionRepository SubdivisionRepository { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }
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

		public IEntityAutocompleteSelectorFactory SalesPlanSelectorFactory { get; }

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

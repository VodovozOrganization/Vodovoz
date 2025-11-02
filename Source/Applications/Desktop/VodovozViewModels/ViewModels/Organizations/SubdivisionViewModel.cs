using Autofac;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.WageCalculation;
using VodovozBusiness.Services.Subdivisions;

namespace Vodovoz.ViewModels.ViewModels.Organizations
{
	public class SubdivisionViewModel : EntityTabViewModelBase<Subdivision>
	{
		private readonly ILifetimeScope _scope;
		private readonly ISubdivisionPermissionsService _subdivisionPermissionsService;
		private readonly IGenericRepository<Subdivision> _subdivisionGenericRepository;
		private readonly ICashRepository _cashRepository;
		private PresetSubdivisionPermissionsViewModel _presetSubdivisionPermissionVm;
		private WarehousePermissionsViewModel _warehousePermissionsVm;
		private bool _canEnablePacs;
		private SubdivisionsJournalViewModel _subdivisionsJournalViewModel;
		private bool _isAddSubdivisionPermissionsSelected;
		private bool _isReplaceSubdivisionPermissionsSelected;

		private FinancialResponsibilityCenter _financialResponsibilityCenter;

		public SubdivisionViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IPermissionRepository permissionRepository,
			ISubdivisionRepository subdivisionRepository,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			ISubdivisionPermissionsService subdivisionPermissionsService,
			IGenericRepository<Subdivision> subdivisionGenericRepository,
			ICashRepository cashRepository,
			SubdivisionsJournalViewModel subdivisionsJournalViewModel,
			ViewModelEEVMBuilder<FinancialResponsibilityCenter> financialResponsibilityCenterViewModelEEVMBuilder)
			: base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			if(financialResponsibilityCenterViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(financialResponsibilityCenterViewModelEEVMBuilder));
			}

			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_subdivisionPermissionsService = subdivisionPermissionsService ?? throw new ArgumentNullException(nameof(subdivisionPermissionsService));
			_subdivisionGenericRepository = subdivisionGenericRepository ?? throw new ArgumentNullException(nameof(subdivisionGenericRepository));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			SubdivisionsJournalViewModel = subdivisionsJournalViewModel ?? throw new ArgumentNullException(nameof(subdivisionsJournalViewModel));
			SubdivisionsJournalViewModel.JournalFilter.SetAndRefilterAtOnce<SubdivisionFilterViewModel>(filter => filter.RestrictParentId = Entity.Id);
			SubdivisionsJournalViewModel.Refresh();
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

			FinancialResponsibilityCenterViewModel = financialResponsibilityCenterViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.FinancialResponsibilityCenter)
				.UseViewModelJournalAndAutocompleter<FinancialResponsibilityCenterJournalViewModel>()
				.UseViewModelDialog<FinancialResponsibilityCenterViewModel>()
				.Finish();

			ConfigureEntityChangingRelations();
			CreateCommands();

			SubscribeUpdateOnChanges();

			AddSubdivisionPermissionsCommand = new DelegateCommand(AddSubdivisionPermissions);
			ReplaceSubdivisionPermissionsCommand = new DelegateCommand(ReplaceSubdivisionPermissions);

			_canEnablePacs = CommonServices.PermissionService.ValidateUserPresetPermission(
				Vodovoz.Core.Domain.Permissions.PacsPermissions.CanEnablePacs,
				CommonServices.UserService.CurrentUserId);

			if(Entity.Id != 0)
			{
				CanArchive = 
					_cashRepository.CurrentCashForGivenSubdivisions(UoW, new int[] { Entity.Id }).Sum(x => x.Balance) == 0 || Entity.IsArchive;
			}
			else
			{
				CanArchive = true;
			}
		}

		public event Action UpdateWarehousePermissionsAction;

		public ISubdivisionRepository SubdivisionRepository { get; }
		public SubdivisionsJournalViewModel SubdivisionsJournalViewModel { get; }
		public IEntityEntryViewModel ChiefViewModel { get; private set; }
		public IEntityEntryViewModel ParentSubdivisionViewModel { get; private set; }
		public IEntityEntryViewModel DefaultSalesPlanViewModel { get; private set; }
		public IEntityEntryViewModel FinancialResponsibilityCenterViewModel { get; }
		public DelegateCommand AddSubdivisionPermissionsCommand { get; }
		public DelegateCommand ReplaceSubdivisionPermissionsCommand { get; }

		public FinancialResponsibilityCenter FinancialResponsibilityCenter
		{
			get => this.GetIdRefField(ref _financialResponsibilityCenter, Entity.FinancialResponsibilityCenterId);
			set => this.SetIdRefField(SetField, ref _financialResponsibilityCenter, () => Entity.FinancialResponsibilityCenterId, value);
		}

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
		public bool CanEnablePacs => _canEnablePacs;

		public bool CanArchive { get; }

		public bool CanAddOrReplacePermissions =>
			CommonServices.UserService.GetCurrentUser().IsAdmin
			&& !Entity.ChildSubdivisions.Any();

		public bool GeographicGroupVisible => Entity.ParentSubdivision != null && Entity.ChildSubdivisions.Any();

		public virtual GeoGroup GeographicGroup
		{
			get => Entity.GeographicGroup;
			set
			{
				if(Entity.GeographicGroup == value)
				{
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

		private void SelectSourceSubdivisionToCopyPermissions()
		{
			var selectSubdivisionPage = NavigationManager.OpenViewModel<SubdivisionsJournalViewModel, Action<SubdivisionFilterViewModel>>(
				this,
				filter =>
				{

				},
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = JournalSelectionMode.Single;
				});

			if(_subdivisionsJournalViewModel != null)
			{
				_subdivisionsJournalViewModel.OnSelectResult -= OnSourceSubdivisionToCopyPermissionsSelected;
			}

			_subdivisionsJournalViewModel = selectSubdivisionPage.ViewModel;
			_subdivisionsJournalViewModel.OnSelectResult += OnSourceSubdivisionToCopyPermissionsSelected;
		}

		private void OnSourceSubdivisionToCopyPermissionsSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.FirstOrDefault();

			if(selectedNode == null
				|| !(selectedNode is SubdivisionJournalNode subdivisionNode)
				|| subdivisionNode.Id == Entity.Id)
			{
				return;
			}

			var subdivision =
				_subdivisionGenericRepository
				.Get(UoW, x => x.Id == subdivisionNode.Id)
				.FirstOrDefault();

			if(subdivision is null)
			{
				return;
			}

			if(_isAddSubdivisionPermissionsSelected)
			{
				AddSubdivisionPermissions(subdivision);
			}

			if(_isReplaceSubdivisionPermissionsSelected)
			{
				ReplaceSubdivisionPermissions(subdivision);
			}
		}

		private void AddSubdivisionPermissions()
		{
			_isAddSubdivisionPermissionsSelected = true;
			_isReplaceSubdivisionPermissionsSelected = false;

			SelectSourceSubdivisionToCopyPermissions();
		}

		private void ReplaceSubdivisionPermissions()
		{
			_isAddSubdivisionPermissionsSelected = false;
			_isReplaceSubdivisionPermissionsSelected = true;

			SelectSourceSubdivisionToCopyPermissions();
		}

		private void AddSubdivisionPermissions(Subdivision sourceSubdivision)
		{
			EntitySubdivisionPermissionViewModel.AddPermissionsFromSubdivision(_subdivisionPermissionsService, sourceSubdivision);
			WarehousePermissionsVM.AddPermissionsFromSubdivision(_subdivisionPermissionsService, Entity, sourceSubdivision);
			PresetSubdivisionPermissionVM.AddPermissionsFromSubdivision(_subdivisionPermissionsService, sourceSubdivision);

			UpdateWarehousePermissionsAction?.Invoke();
		}

		private void ReplaceSubdivisionPermissions(Subdivision sourceSubdivision)
		{
			EntitySubdivisionPermissionViewModel.ReplacePermissionsFromSubdivision(_subdivisionPermissionsService, sourceSubdivision);
			WarehousePermissionsVM.ReplacePermissionsFromSubdivision(_subdivisionPermissionsService, Entity, sourceSubdivision);
			PresetSubdivisionPermissionVM.ReplacePermissionsFromSubdivision(_subdivisionPermissionsService, sourceSubdivision);

			UpdateWarehousePermissionsAction?.Invoke();
		}

		public override void Dispose()
		{
			if(_subdivisionsJournalViewModel != null)
			{
				_subdivisionsJournalViewModel.OnSelectResult -= OnSourceSubdivisionToCopyPermissionsSelected;
			}

			UnsubscribeUpdateOnChanges();
			base.Dispose();
		}
	}
}

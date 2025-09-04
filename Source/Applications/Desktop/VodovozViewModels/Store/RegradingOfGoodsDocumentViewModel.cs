using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Warehouses;
using VodovozBusiness.Domain.Documents;

namespace Vodovoz.ViewModels.Store
{
	public class RegradingOfGoodsDocumentViewModel : EntityTabViewModelBase<RegradingOfGoodsDocument>, IDisposable
	{
		private readonly ILogger<RegradingOfGoodsDocumentViewModel> _logger;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IUserRepository _userRepository;
		private readonly IStoreDocumentHelper _storeDocumentHelper;
		private bool _canEditItems;
		private bool _canSave;

		public RegradingOfGoodsDocumentViewModel(
			ILogger<RegradingOfGoodsDocumentViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeRepository employeeRepository,
			IUserRepository userRepository,
			IStoreDocumentHelper storeDocumentHelper,
			ViewModelEEVMBuilder<Warehouse> warehouseViewModelEEVMBuilder,
			RegradingOfGoodsDocumentItemsViewModel regradingOfGoodsDocumentItemsViewModel)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(warehouseViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(warehouseViewModelEEVMBuilder));
			}

			if(regradingOfGoodsDocumentItemsViewModel is null)
			{
				throw new ArgumentNullException(nameof(regradingOfGoodsDocumentItemsViewModel));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_storeDocumentHelper = storeDocumentHelper ?? throw new ArgumentNullException(nameof(storeDocumentHelper));

			if(uowBuilder.IsNewEntity)
			{
				Entity.AuthorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id
					?? throw new AbortCreatingPageException(
						"Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.",
						"Ошибка");

				Entity.Warehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.RegradingOfGoodsEdit);
			}

			if(_storeDocumentHelper.CheckAllPermissions(uowBuilder.IsNewEntity, WarehousePermissionsType.RegradingOfGoodsEdit, Entity.Warehouse))
			{
				throw new AbortCreatingPageException(
					"У вас нет доступа к этому документу",
					"Ошибка");
			}

			CanSave = true;
			CanEditItems = _storeDocumentHelper.CanEditDocument(WarehousePermissionsType.RegradingOfGoodsEdit, Entity.Warehouse);

			var userHasOnlyAccessToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;

			var availableWarehousesIds =
				_storeDocumentHelper.GetRestrictedWarehousesIds(UoW, WarehousePermissionsType.RegradingOfGoodsEdit);

			WarehouseViewModel = warehouseViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
				{
					filter.IncludeWarehouseIds = availableWarehousesIds;
				})
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			WarehouseViewModel.IsEditable = !userHasOnlyAccessToWarehouseAndComplaints;

			if(Entity.Items.Count > 0)
			{
				WarehouseViewModel.IsEditable = false;
			}

			var permmissionValidator = new EntityExtendedPermissionValidator(unitOfWorkFactory, PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);

			Entity.CanEdit = permmissionValidator.Validate(typeof(RegradingOfGoodsDocument), _userRepository.GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));

			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Today)
			{
				CanEditItems = false;
				CanSave = false;
			}
			else
			{
				Entity.CanEdit = true;
			}

			ItemsViewModel = regradingOfGoodsDocumentItemsViewModel;
			ItemsViewModel.SetUnitOfWork(UoW);
			ItemsViewModel.Items = Entity.Items;
			ItemsViewModel.CurrentWarehouse = Entity.Warehouse;
			ItemsViewModel.ParentViewModel = this;

			Entity.PropertyChanged += OnEntityPropertyChanged;

			SaveCommand = new DelegateCommand(SaveAndClose, () => CanSave);
			CancelCommand = new DelegateCommand(() => Close(true, CloseSource.Cancel));
		}

		public bool CanSave
		{
			get => _canSave;
			private set => SetField(ref _canSave, value);
		}

		public bool CanEditItems
		{
			get => _canEditItems;
			private set => SetField(ref _canEditItems, value);
		}

		public IEntityEntryViewModel WarehouseViewModel { get; }
		public RegradingOfGoodsDocumentItemsViewModel ItemsViewModel { get; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Warehouse))
			{
				ItemsViewModel.CurrentWarehouse = Entity.Warehouse;
				CanEditItems = _storeDocumentHelper.CanEditDocument(WarehousePermissionsType.RegradingOfGoodsEdit, Entity.Warehouse);
			}
		}

		protected override bool BeforeSave()
		{
			if(!Entity.CanEdit)
			{
				return false;
			}

			Entity.LastEditorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			Entity.LastEditedTime = DateTime.Now;

			if(Entity.LastEditorId == null)
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			return true;
		}

		public override void Dispose()
		{
			ItemsViewModel?.Dispose();
			base.Dispose();
		}
	}
}

using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Warehouses;
using DocumentTypeEnum = Vodovoz.Core.Domain.Warehouses.Documents.DocumentType;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseDocumentsJournalFilterViewModel : FilterViewModelBase<WarehouseDocumentsJournalFilterViewModel>
	{
		private readonly ICommonServices _commonServices;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IGenericRepository<Warehouse> _warehouseRepository;
		private readonly ViewModelEEVMBuilder<Warehouse> _warehouseEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _driverEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _employeeEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Car> _carEEVMBuilder;
		private WarehouseDocumentsJournalViewModel _journal;

		private DocumentType? _documentType;
		private Warehouse _warehouse;
		private MovementDocumentStatus? _movementDocumentStatus;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private Employee _employee;
		private Car _car;
		private bool _canChangeRestrictedDocumentType = true;
		private bool _onlyQRScanRequiredCarLoadDocuments;

		public WarehouseDocumentsJournalFilterViewModel(
			ICommonServices commonServices,
			IUserSettingsService userSettingsService,
			IGenericRepository<Warehouse> warehouseRepository,
			ViewModelEEVMBuilder<Warehouse> warehouseEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverEEVMBuilder,
			ViewModelEEVMBuilder<Employee> employeeEEVMBuilder,
			ViewModelEEVMBuilder<Car> carEEVMBuilder)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_warehouseEEVMBuilder = warehouseEEVMBuilder ?? throw new ArgumentNullException(nameof(warehouseEEVMBuilder));
			_driverEEVMBuilder = driverEEVMBuilder ?? throw new ArgumentNullException(nameof(driverEEVMBuilder));
			_employeeEEVMBuilder = employeeEEVMBuilder ?? throw new ArgumentNullException(nameof(employeeEEVMBuilder));
			_carEEVMBuilder = carEEVMBuilder ?? throw new ArgumentNullException(nameof(carEEVMBuilder));

			StartDate = DateTime.Today.AddDays(-7);
			EndDate = DateTime.Today.AddDays(1);
		}

		public bool OnlyQRScanRequiredCarLoadDocuments
		{
			get => _onlyQRScanRequiredCarLoadDocuments;
			set => UpdateFilterField(ref _onlyQRScanRequiredCarLoadDocuments, value);
		}

		public bool IsUserHasAccessNotOnlyToWarehouseAndComplaints =>
			!_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
			|| _commonServices.UserService.GetCurrentUser().IsAdmin;

		public bool CanSelectMovementStatus => DocumentType == DocumentTypeEnum.MovementDocument;
		public bool ShowOnlyQRScanRequiredCarLoadDocuments => DocumentType == DocumentTypeEnum.CarLoadDocument;

		public IEntityEntryViewModel WarehouseEntityEntryViewModel { get; private set; }
		public IEntityEntryViewModel DriverEntityEntryViewModel { get; private set; }
		public IEntityEntryViewModel EmployeeEntityEntryViewModel { get; private set; }
		public IEntityEntryViewModel CarEntityEntryViewModel { get; private set; }
		public object[] DocumentTypesNotAllowedToSelect => new object[] { DocumentTypeEnum.DeliveryDocument };

		public WarehouseDocumentsJournalViewModel Journal
		{
			get => _journal;
			set
			{
				if(value is null)
				{
					throw new InvalidOperationException($"Устанавливаемое значение свойства {nameof(Journal)} не должно быть null");
				}

				if(_journal != null)
				{
					throw new InvalidOperationException($"Свойство {nameof(Journal)} уже установлено!");
				}

				SetField(ref _journal, value);

				SetDriverEntityEntryViewModel();
				SetWarehouseEntityEntryViewModel();
				SetEmployeeEntityEntryViewModel();
				SetCarEntityEntryViewModel();
			}
		}

		[PropertyChangedAlso(nameof(CanSelectMovementStatus))]
		[PropertyChangedAlso(nameof(OnlyQRScanRequiredCarLoadDocuments))]
		[PropertyChangedAlso(nameof(ShowOnlyQRScanRequiredCarLoadDocuments))]
		public DocumentType? DocumentType
		{
			get => _documentType;
			set
			{
				if(UpdateFilterField(ref _documentType, value))
				{
					MovementDocumentStatus = null;
				}
			}
		}

		public Warehouse Warehouse
		{
			get => _warehouse;
			set => UpdateFilterField(ref _warehouse, value);
		}

		public MovementDocumentStatus? MovementDocumentStatus
		{
			get => _movementDocumentStatus;
			set => UpdateFilterField(ref _movementDocumentStatus, value);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public Employee Driver
		{
			get => _driver;
			set => UpdateFilterField(ref _driver, value);
		}

		public Employee Employee
		{
			get => _employee;
			set => UpdateFilterField(ref _employee, value);
		}

		public Car Car
		{
			get => _car;
			set => UpdateFilterField(ref _car, value);
		}

		public bool CanChangeRestrictedDocumentType
		{
			get => _canChangeRestrictedDocumentType;
			set => SetField(ref _canChangeRestrictedDocumentType, value);
		}

		private void SetDriverEntityEntryViewModel()
		{
			if(DriverEntityEntryViewModel != null)
			{
				throw new InvalidOperationException($"Свойство {nameof(DriverEntityEntryViewModel)} уже установлено!");
			}

			if(Journal is null)
			{
				throw new InvalidOperationException($"Свойство фильтра {nameof(Journal)} не установлено!");
			}

			var driverEntityEntryViewModel = _driverEEVMBuilder
					.SetViewModel(Journal)
					.SetUnitOfWork(Journal.UoW)
					.ForProperty(this, x => x.Driver)
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
					{
						filter.RestrictCategory = Core.Domain.Employees.EmployeeCategory.driver;
						filter.Status = Core.Domain.Employees.EmployeeStatus.IsWorking;
					})
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();

			driverEntityEntryViewModel.CanViewEntity =
				IsUserHasAccessNotOnlyToWarehouseAndComplaints
				&& _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Warehouse)).CanUpdate;

			DriverEntityEntryViewModel = driverEntityEntryViewModel;
		}

		private void SetWarehouseEntityEntryViewModel()
		{
			if(WarehouseEntityEntryViewModel != null)
			{
				throw new InvalidOperationException($"Свойство {nameof(WarehouseEntityEntryViewModel)} уже установлено!");
			}

			if(Journal is null)
			{
				throw new InvalidOperationException($"Свойство фильтра {nameof(Journal)} не установлено!");
			}

			var warehouseEntityEntryViewModel = _warehouseEEVMBuilder
					.SetViewModel(Journal)
					.SetUnitOfWork(Journal.UoW)
					.ForProperty(this, x => x.Warehouse)
					.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
					{
					})
					.UseViewModelDialog<WarehouseViewModel>()
					.Finish();

			warehouseEntityEntryViewModel.CanViewEntity =
				_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Employee)).CanUpdate;

			WarehouseEntityEntryViewModel = warehouseEntityEntryViewModel;
			SetDefaultWarehouse(Journal.UoW);
		}

		private void SetDefaultWarehouse(IUnitOfWork uow)
		{
			var defaultWarehouse = _userSettingsService.Settings.DefaultWarehouse;

			if(defaultWarehouse == null)
			{
				return;
			}

			Warehouse = _warehouseRepository.Get(uow, w => w.Id == defaultWarehouse.Id).FirstOrDefault();
		}

		private void SetEmployeeEntityEntryViewModel()
		{
			if(EmployeeEntityEntryViewModel != null)
			{
				throw new InvalidOperationException($"Свойство {nameof(EmployeeEntityEntryViewModel)} уже установлено!");
			}

			if(Journal is null)
			{
				throw new InvalidOperationException($"Свойство фильтра {nameof(Journal)} не установлено!");
			}

			var viewModel = _employeeEEVMBuilder
					.SetViewModel(Journal)
					.SetUnitOfWork(Journal.UoW)
					.ForProperty(this, x => x.Employee)
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
					{
						filter.Status = Core.Domain.Employees.EmployeeStatus.IsWorking;
					})
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();

			viewModel.CanViewEntity =
				IsUserHasAccessNotOnlyToWarehouseAndComplaints
				&& _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Warehouse)).CanUpdate;

			EmployeeEntityEntryViewModel = viewModel;
		}

		private void SetCarEntityEntryViewModel()
		{
			if(CarEntityEntryViewModel != null)
			{
				throw new InvalidOperationException($"Свойство {nameof(CarEntityEntryViewModel)} уже установлено!");
			}

			if(Journal is null)
			{
				throw new InvalidOperationException($"Свойство фильтра {nameof(Journal)} не установлено!");
			}

			var viewModel = _carEEVMBuilder
					.SetViewModel(Journal)
					.SetUnitOfWork(Journal.UoW)
					.ForProperty(this, x => x.Car)
					.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(filter =>
					{
					})
					.UseViewModelDialog<CarViewModel>()
					.Finish();

			viewModel.CanViewEntity =
				IsUserHasAccessNotOnlyToWarehouseAndComplaints
				&& _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			CarEntityEntryViewModel = viewModel;
		}
	}
}

﻿using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseDocumentsJournalFilterViewModel : FilterViewModelBase<WarehouseDocumentsJournalFilterViewModel>
	{
		private readonly ICommonServices _commonServices;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IGenericRepository<Warehouse> _warehouseRepository;
		private readonly ViewModelEEVMBuilder<Warehouse> _warehouseEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _driverEEVMBuilder;

		private WarehouseDocumentsJournalViewModel _journal;

		private DocumentType? _documentType;
		private Warehouse _warehouse;
		private MovementDocumentStatus? _movementDocumentStatus;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
		private bool _canChangeRestrictedDocumentType = true;

		public WarehouseDocumentsJournalFilterViewModel(
			ICommonServices commonServices,
			IUserSettingsService userSettingsService,
			IGenericRepository<Warehouse> warehouseRepository,
			ViewModelEEVMBuilder<Warehouse> warehouseEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverEEVMBuilder)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_warehouseEEVMBuilder = warehouseEEVMBuilder ?? throw new ArgumentNullException(nameof(warehouseEEVMBuilder));
			_driverEEVMBuilder = driverEEVMBuilder ?? throw new ArgumentNullException(nameof(driverEEVMBuilder));


			StartDate = DateTime.Today.AddDays(-7);
			EndDate = DateTime.Today.AddDays(1);
		}

		public bool CanSelectWarehouse =>
			!_commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
			|| _commonServices.UserService.GetCurrentUser().IsAdmin;

		public bool CanSelectMovementStatus => DocumentType == Domain.Documents.DocumentType.MovementDocument;

		public IEntityEntryViewModel WarehouseEntityEntryViewModel { get; private set; }
		public IEntityEntryViewModel DriverEntityEntryViewModel { get; private set; }

		public WarehouseDocumentsJournalViewModel Journal
		{
			get => _journal;
			set
			{
				if(_journal != null)
				{
					return;
				}

				SetField(ref _journal, value);

				var driverEntityEntryViewModel = _driverEEVMBuilder
					.SetViewModel(value)
					.SetUnitOfWork(value.UoW)
					.ForProperty(this, x => x.Driver)
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
					{
						filter.RestrictCategory = Core.Domain.Employees.EmployeeCategory.driver;
						filter.Status = Core.Domain.Employees.EmployeeStatus.IsWorking;
					})
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();

				driverEntityEntryViewModel.CanViewEntity =
					CanSelectWarehouse
					&& _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Warehouse)).CanUpdate;

				DriverEntityEntryViewModel = driverEntityEntryViewModel;

				var warehouseEntityEntryViewModel = _warehouseEEVMBuilder
					.SetViewModel(value)
					.SetUnitOfWork(value.UoW)
					.ForProperty(this, x => x.Warehouse)
					.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(filter =>
					{
					})
					.UseViewModelDialog<WarehouseViewModel>()
					.Finish();

				warehouseEntityEntryViewModel.CanViewEntity =
					_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Employee)).CanUpdate;

				WarehouseEntityEntryViewModel = warehouseEntityEntryViewModel;
				SetDefaultWarehouse(value.UoW);
			}
		}

		[PropertyChangedAlso(nameof(CanSelectMovementStatus))]
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

		public bool CanChangeRestrictedDocumentType
		{
			get => _canChangeRestrictedDocumentType;
			set => SetField(ref _canChangeRestrictedDocumentType, value);
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
	}
}

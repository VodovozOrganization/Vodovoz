using QS.DomainModel.Entity;
using QS.Project.Filter;
using QS.Services;
using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.Specifications;
using Vodovoz.Specifications.Store.Documents;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.ViewModels.Flyers;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseDocumentsItemsJournalFilterViewModel : FilterViewModelBase<WarehouseDocumentsItemsJournalFilterViewModel>
	{
		private const string _haveAccessOnlyToWarehouseAndComplaintsPermissionName = "user_have_access_only_to_warehouse_and_complaints";

		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IUserService _userService;
		private readonly IUserRepository _userRepository;
		private Warehouse _warehouse;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private DocumentType? _restrictDocumentType;
		private MovementDocumentStatus? _movementDocumentStatus;
		private MovementDocumentStatus? _restrictMovementStatus;
		private Warehouse _restrictWarehouse;
		private DateTime? _restrictStartDate;
		private DateTime? _restrictEndDate;
		private Employee _driver;
		private Employee _restrictDriver;
		private DocumentType? _documentType;

		public WarehouseDocumentsItemsJournalFilterViewModel(
			IWarehouseJournalFactory warehouseJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICurrentPermissionService currentPermissionService,
			IUserService userService,
			IUserRepository userRepository)
		{
			WarehouseJournalFactory = warehouseJournalFactory ?? throw new ArgumentNullException(nameof(warehouseJournalFactory));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));

			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

			Warehouse = _userRepository.GetCurrentUserSettings(UoW).DefaultWarehouse;

			StartDate = DateTime.Today.AddDays(-7);
			EndDate = DateTime.Today.AddDays(1);
		}

		public IWarehouseJournalFactory WarehouseJournalFactory { get; }

		public IEmployeeJournalFactory EmployeeJournalFactory { get; }

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

		public TimestampBetweenSpecification<TDocument> GetPeriodSpecification<TDocument>()
			where TDocument : Document
		{
			return new TimestampBetweenSpecification<TDocument>(StartDate, EndDate.Value.AddDays(1));
		}

		public Warehouse Warehouse
		{
			get => _warehouse;
			set => UpdateFilterField(ref _warehouse, value);
		}

		public WarehouseIdSpecification<TDocument> GetWarehouseSpecification<TDocument>()
			where TDocument : Document, IWarehouseBindedDocument
		{
			return new WarehouseIdSpecification<TDocument>(Warehouse?.Id);
		}

		public Employee Driver
		{
			get => _driver;
			set => UpdateFilterField(ref _driver, value);
		}

		public DocumentType? DocumentType
		{
			get => _documentType;
			set => UpdateFilterField(ref _documentType, value);
		}

		public MovementDocumentStatus? MovementDocumentStatus
		{
			get => _movementDocumentStatus;
			set => UpdateFilterField(ref _movementDocumentStatus, value);
		}

		[PropertyChangedAlso(nameof(CanChangeDatePeriod))]
		public DateTime? RestrictStartDate
		{
			get => _restrictStartDate;
			set
			{
				if(UpdateFilterField(ref _restrictStartDate, value) && value != null)
				{
					StartDate = value.Value;
				}
			}
		}

		[PropertyChangedAlso(nameof(CanChangeDatePeriod))]
		public DateTime? RestrictEndDate
		{
			get => _restrictEndDate;
			set
			{
				if(UpdateFilterField(ref _restrictEndDate, value) && value != null)
				{
					EndDate = value.Value;
				}
			}
		}

		[PropertyChangedAlso(nameof(CanChangeWarehouse))]
		public Warehouse RestrictWarehouse
		{
			get => _restrictWarehouse;
			set
			{
				if(UpdateFilterField(ref _restrictWarehouse, value) && value != null)
				{
					Warehouse = value;
				}
			}
		}

		[PropertyChangedAlso(nameof(CanChangeDocumentType))]
		public DocumentType? RestrictDocumentType
		{
			get => _restrictDocumentType;
			set
			{
				if(UpdateFilterField(ref _restrictDocumentType, value) && value != null)
				{
					DocumentType = value;
				}
			}
		}

		[PropertyChangedAlso(nameof(CanChangeMovementDocumentStatus))]
		public MovementDocumentStatus? RestrictMovementStatus
		{
			get => _restrictMovementStatus;
			set
			{
				if(UpdateFilterField(ref _restrictMovementStatus, value) && value != null)
				{
					MovementDocumentStatus = value;
				}
			}
		}

		[PropertyChangedAlso(nameof(CanChangeDriver))]
		public Employee RestrictDriver
		{
			get => _restrictDriver;
			set
			{
				if(UpdateFilterField(ref _restrictDriver, value) && value != null)
				{
					Driver = value;
				}
			}
		}

		public bool CanChangeDatePeriod => RestrictStartDate is null && RestrictEndDate is null;

		public bool CanChangeWarehouse => CanReadWarehouse && (RestrictWarehouse is null);

		public bool CanReadWarehouse => !_currentPermissionService.ValidatePresetPermission(_haveAccessOnlyToWarehouseAndComplaintsPermissionName) || _userService.GetCurrentUser(UoW).IsAdmin;

		public bool CanUpdateWarehouse => CanReadWarehouse;

		public bool ShowMovementDocumentFilterDetails => DocumentType.HasValue && (DocumentType.Value == Domain.Documents.DocumentType.MovementDocument);

		public bool CanChangeDocumentType => RestrictDocumentType is null;

		public bool CanChangeMovementDocumentStatus => RestrictMovementStatus is null;

		public bool CanChangeDriver => RestrictDriver is null;
	}
}

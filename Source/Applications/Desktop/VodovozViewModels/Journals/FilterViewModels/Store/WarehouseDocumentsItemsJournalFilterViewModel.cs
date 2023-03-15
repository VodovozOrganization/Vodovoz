using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.Project.Filter;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.NHibernateProjections.Logistics;
using Vodovoz.Specifications.Store.Documents;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Reports;

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
		private TargetSource _targetSource;
		private SelectableParameterReportFilterViewModel _filterViewModel;
		private readonly SelectableParametersReportFilter _filter;

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

			StartDate = DateTime.Today.AddDays(-7);
			EndDate = DateTime.Today.AddDays(1);

			_filter = new SelectableParametersReportFilter(UoW);
			ConfigureFilter();
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

		//public ISpecification<TDocument> GetTwoWarhousesSpecification<TDocument>()
		//	where TDocument : ITwoWarhousesBindedDocument
		//{
		//	return new DocumentTwoWarehousesBoundedIdSpecification<TDocument>(Warehouse?.Id);
		//}

		//public ISpecification<TDocument> GetWarehouseSpecification<TDocument>()
		//	where TDocument : IWarehouseBoundedDocument
		//{
		//	return new DocumentOneWarehouseBoundedIdSpecification<TDocument>(Warehouse?.Id);
		//}

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

		public TargetSource TargetSource
		{
			get => _targetSource;
			set => UpdateFilterField(ref _targetSource, value);
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

		public virtual SelectableParameterReportFilterViewModel FilterViewModel
		{
			get => _filterViewModel;
			set => SetField(ref _filterViewModel, value);
		}

		public bool CanChangeDatePeriod => RestrictStartDate is null && RestrictEndDate is null;

		//public bool CanChangeWarehouse => CanReadWarehouse && (RestrictWarehouse is null);

		public bool CanReadWarehouse => !_currentPermissionService.ValidatePresetPermission(_haveAccessOnlyToWarehouseAndComplaintsPermissionName) || _userService.GetCurrentUser(UoW).IsAdmin;

		public bool CanUpdateWarehouse => CanReadWarehouse;

		public bool ShowMovementDocumentFilterDetails => DocumentType.HasValue && (DocumentType.Value == Domain.Documents.DocumentType.MovementDocument);

		public bool CanChangeDocumentType => RestrictDocumentType is null;

		public bool CanChangeMovementDocumentStatus => RestrictMovementStatus is null;

		public bool CanChangeDriver => RestrictDriver is null;

		private void ConfigureFilter()
		{
			_filter.CreateParameterSet(
				"Контрагент",
				nameof(Counterparty),
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Counterparty> resultAlias = null;
					var query = UoW.Session.QueryOver<Counterparty>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.FullName).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Counterparty>>());
					return query.List<SelectableParameter>();
				}));

			_filter.CreateParameterSet(
				"Склад",
				nameof(Warehouse),
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Warehouse> resultAlias = null;
					var query = UoW.Session.QueryOver<Warehouse>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Warehouse>>());
					return query.List<SelectableParameter>();
				}));

			_filter.CreateParameterSet(
				"Водитель",
				nameof(Employee),
				new ParametersFactory(UoW, (filters) =>
				{
					Employee driverAlias = null;

					SelectableEntityParameter<Employee> resultAlias = null;
					var query = UoW.Session.QueryOver(() => driverAlias)
						.Where(x => x.Category == EmployeeCategory.driver);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(EmployeeProjections.GetDriverFullNamePojection()).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
					return query.List<SelectableParameter>();
				}));

			_filter.CreateParameterSet(
				"Автомобиль",
				nameof(Car),
				new ParametersFactory(UoW, (filters) =>
				{
					Car carAlias = null;
					CarModel carModelAlias = null;
					CarManufacturer carManufacturerAlias = null;
					Employee driverAlias = null;

					SelectableEntityParameter<Car> resultAlias = null;
					var query = UoW.Session.QueryOver(() => carAlias)
						.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
						.Left.JoinAlias(() => carModelAlias.CarManufacturer, () => carManufacturerAlias)
						.Left.JoinAlias(() => carAlias.Driver, () => driverAlias)
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(CarProjections.GetCarTitleProjection()).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Car>>());
					return query.List<SelectableParameter>();
				}));

			_filter.CreateParameterSet(
				"Фура",
				nameof(MovementWagon),
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<MovementWagon> resultAlias = null;
					var query = UoW.Session.QueryOver<MovementWagon>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<MovementWagon>>());
					return query.List<SelectableParameter>();
				}));

			FilterViewModel = new SelectableParameterReportFilterViewModel(_filter);
		}
	}

	public enum TargetSource
	{
		Source,
		Target,
		Both
	}
}

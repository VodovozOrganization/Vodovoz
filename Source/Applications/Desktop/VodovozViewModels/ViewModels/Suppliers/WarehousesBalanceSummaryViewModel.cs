using ClosedXML.Excel;
using DateTimeHelpers;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Multi;
using NHibernate.Transform;
using NHibernate.Util;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.NHibernateProjections.Goods;
using Vodovoz.ViewModels.Reports;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class WarehousesBalanceSummaryViewModel : DialogTabViewModelBase
	{
		private const int _firstRow = 1;
		public const string RowNumberTitle = "№";
		public const string IdTitle = "Код";
		public const string EntityTitle = "Наименование";
		public const string InventoryNumberTitle = "Инвентарный\nномер";
		public const string BalanceTitle = "Остаток";
		public const string ParameterWarehouseStorages = nameof(Warehouse);
		public const string ParameterEmployeeStorages = nameof(Employee);
		public const string ParameterCarStorages = nameof(Car);
		private const string _storage = "Хранилище";
		private const string _xlsxFileFilter = "XLSX File (*.xlsx)";
		private const string _parameterNomenclature = nameof(Nomenclature);
		private const string _parameterNomenclatureType = nameof(NomenclatureCategory);
		private const string _parameterProductGroups = nameof(ProductGroup);
		private const string _parameterInstances = nameof(InventoryNomenclatureInstance);
		private const string _bulkBalanceWarehousesKey = "bulkBalanceWarehouses";
		private const string _bulkBalanceEmployeesKey = "bulkBalanceEmployees";
		private const string _bulkBalanceCarsKey = "bulkBalanceCars";
		private const string _instanceBalanceWarehousesKey = "instanceBalanceWarehouses";
		private const string _instanceBalanceEmployeesKey = "instanceBalanceEmployees";
		private const string _instanceBalanceCarsKey = "instanceBalanceCars";
		private const string _minStockKey = "minStock";
		private const string _instanceDataKey = "instanceData";
		private readonly IFileDialogService _fileDialogService;
		private SelectableParameterReportFilterViewModel _nomsViewModel;
		private SelectableParameterReportFilterViewModel _storagesViewModel;
		private SelectableParametersReportFilter _nomsFilter;
		private SelectableParametersReportFilter _storagesFilter;

		private DateTime? _endDate = DateTime.Today;
		private bool _showReserve;
		private bool _showPrices;
		private bool _isGreaterThanZeroByNomenclature;
		private bool _isGreaterThanZeroByWarehouse;
		private bool _groupingActiveStorage;
		private bool _isGenerating = false;
		private BalanceSummaryReport _balanceSummaryReport;
		private ActiveStoragesBalanceSummaryReport _activeStoragesBalanceSummaryReport;
		private bool _isCreatedWithReserveData = false;

		public WarehousesBalanceSummaryViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_fileDialogService = fileDialogService;
			TabName = "Остатки";
		}

		#region Свойства

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public bool ShowReserve
		{
			get => _showReserve;
			set
			{
				SetField(ref _showReserve, value);
				EndDate = DateTime.Now;
			}
		}

		public bool ShowPrices
		{
			get => _showPrices;
			set => SetField(ref _showPrices, value);
		}

		public bool AllNomenclatures { get; set; } = true;

		public bool IsGreaterThanZeroByNomenclature
		{
			get => _isGreaterThanZeroByNomenclature;
			set => SetField(ref _isGreaterThanZeroByNomenclature, value);
		}
		public bool IsLessOrEqualZeroByNomenclature { get; set; }
		public bool IsLessThanMinByNomenclature { get; set; }
		public bool IsGreaterOrEqualThanMinByNomenclature { get; set; }

		public bool AllWarehouses { get; set; } = true;

		public bool IsGreaterThanZeroByWarehouse
		{
			get => _isGreaterThanZeroByWarehouse;
			set => SetField(ref _isGreaterThanZeroByWarehouse, value);
		}
		public bool IsLessOrEqualZeroByWarehouse { get; set; }
		public bool IsLessThanMinByWarehouse { get; set; }
		public bool IsGreaterOrEqualThanMinByWarehouse { get; set; }
		
		public IList<SelectableParameterSet> StoragesParametersSets = new List<SelectableParameterSet>();

		public SelectableParameterReportFilterViewModel NomsViewModel => _nomsViewModel ?? (_nomsViewModel = CreateNomsViewModel());

		public SelectableParameterReportFilterViewModel StoragesViewModel =>
			_storagesViewModel ?? (_storagesViewModel = CreateStoragesViewModel());

		public bool IsGenerating
		{
			get => _isGenerating;
			set => SetField(ref _isGenerating, value);
		}

		public CancellationTokenSource ReportGenerationCancellationTokenSource { get; set; }

		public BalanceSummaryReport BalanceSummaryReport
		{
			get => _balanceSummaryReport;
			set
			{
				if(!SetField(ref _balanceSummaryReport, value))
				{
					return;
				}

				if(_balanceSummaryReport is null)
				{
					return;
				}

				ActiveStoragesBalanceSummaryReport = null;
			}
		}

		public ActiveStoragesBalanceSummaryReport ActiveStoragesBalanceSummaryReport
		{
			get => _activeStoragesBalanceSummaryReport;
			set
			{
				if(!SetField(ref _activeStoragesBalanceSummaryReport, value))
				{
					return;
				}

				if(_activeStoragesBalanceSummaryReport is null)
				{
					return;
				}
					
				BalanceSummaryReport = null;
			}
		}

		public bool GroupingActiveStorage
		{
			get => _groupingActiveStorage;
			set
			{
				if(SetField(ref _groupingActiveStorage, value))
				{
					UpdateGroupingActiveStorageState();
				}
			}
		}
		
		public bool Sensitivity => !GroupingActiveStorage;
		
		public StorageType? ActiveSelectedStorageType { get; private set; }

		#endregion

		public void ShowWarning(string message)
		{
			ShowWarningMessage(message);
		}

		public async Task<BalanceSummaryReport> ActionGenerateReportAsync(CancellationToken cancellationToken)
		{
			var uow = UnitOfWorkFactory.CreateWithoutRoot("Отчет остатков по складам");
			try
			{
				return await GenerateBalanceSummaryReportAsync(EndDate ?? DateTime.Today, ShowReserve, ShowPrices, uow, cancellationToken);
			}
			finally
			{
				uow.Dispose();
			}
		}
		
		public async Task<ActiveStoragesBalanceSummaryReport> GenerateActiveStoragesBalanceSummaryReportAsync(CancellationToken cancellationToken)
		{
			var uow = UnitOfWorkFactory.CreateWithoutRoot("Отчет остатков по складам");
			try
			{
				return await GenerateActiveStoragesBalanceSummaryReportAsync(EndDate ?? DateTime.Today, uow, cancellationToken);
			}
			finally
			{
				uow.Dispose();
			}
		}

		#region GenerateDefaultReport

		private async Task<BalanceSummaryReport> GenerateBalanceSummaryReportAsync(
			DateTime endDate,
			bool createReportWithReserveData,
			bool withPrices,
			IUnitOfWork localUow,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			//Флаг типа отчета для экспорта в Эксель. Если выполнять проверку по ShowReserve,
			//то если после формирования отчета переключить чекбокс и нажать экспорт, отчет выгрузится неправильно
			_isCreatedWithReserveData = createReportWithReserveData;

			endDate = endDate.LatestDayTime();

			var parameters = new WarehousesBalanceSummaryReportParameters()
				.AddNomenclaturesSet(_nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNomenclature))
				.AddInstancesSet(_nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterInstances))
				.AddNomenclatureTypesSet(_nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNomenclatureType))
				.AddNomenclatureGroupsSet(_nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterProductGroups))
				.AddWarehouseStorages(_storagesFilter, ParameterWarehouseStorages)
				.AddEmployeeStorages(_storagesFilter, ParameterEmployeeStorages)
				.AddCarStorages(_storagesFilter, ParameterCarStorages);

			Nomenclature nomAlias = null;

			if(!parameters.NomenclaturesSelected && !parameters.InstancesSelected)
			{
				parameters.Nomenclatures.AddRange(parameters.NomenclaturesSet.Parameters);
				parameters.Instances.AddRange(parameters.InstancesSet.Parameters);
			}

			var defaultReport = new BalanceSummaryReport
			{
				EndDate = endDate,
				WarehouseStoragesTitles = parameters.WarehouseStorages?.Select(x => x.Title).ToList(),
				EmployeeStoragesTitles = parameters.EmployeeStorages?.Select(x => x.Title).ToList(),
				CarStoragesTitles = parameters.CarStorages?.Select(x => x.Title).ToList(),
				SummaryRows = new List<BalanceSummaryRow>()
			};

			#region Запросы

			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeBulkGoodsAccountingOperation employeeBulkOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarBulkGoodsAccountingOperation carBulkOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			InventoryNomenclatureInstance instanceAlias = null;
			BalanceBean resultAlias = null;
			InstanceData instanceDataAlias = null;

			IQueryOver<WarehouseBulkGoodsAccountingOperation, WarehouseBulkGoodsAccountingOperation> bulkBalanceByWarehousesQuery = null;
			IQueryOver<WarehouseInstanceGoodsAccountingOperation, WarehouseInstanceGoodsAccountingOperation>
				instanceBalanceByWarehousesQuery = null;

			if(parameters.WarehousesIds.Any())
			{
				bulkBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseBulkOperationAlias)
					.Where(() => warehouseBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseBulkOperationAlias.Warehouse.Id).IsIn(parameters.WarehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseBulkOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => nomAlias.Name).WithAlias(() => resultAlias.EntityName)
						.SelectSum(() => warehouseBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => warehouseBulkOperationAlias.Warehouse.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());

				instanceBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseInstanceOperationAlias)
					.JoinAlias(() => warehouseInstanceOperationAlias.InventoryNomenclatureInstance, () => instanceAlias)
					.Where(() => warehouseInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !instanceAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseInstanceOperationAlias.Warehouse.Id).IsIn(parameters.WarehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseInstanceOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.EntityId)
						.SelectSum(() => warehouseInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => instanceAlias.Id).Asc
					.ThenBy(() => warehouseInstanceOperationAlias.Warehouse.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			IQueryOver<EmployeeBulkGoodsAccountingOperation, EmployeeBulkGoodsAccountingOperation> bulkBalanceByEmployeesQuery = null;
			IQueryOver<EmployeeInstanceGoodsAccountingOperation, EmployeeInstanceGoodsAccountingOperation> instanceBalanceByEmployeesQuery =
				null;

			if(parameters.EmployeesIds.Any())
			{
				bulkBalanceByEmployeesQuery = localUow.Session.QueryOver(() => employeeBulkOperationAlias)
					.Where(() => employeeBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeBulkOperationAlias.Employee.Id).IsIn(parameters.EmployeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeBulkOperationAlias.Employee.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => nomAlias.Name).WithAlias(() => resultAlias.EntityName)
						.SelectSum(() => employeeBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => employeeBulkOperationAlias.Employee.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());

				instanceBalanceByEmployeesQuery = localUow.Session.QueryOver(() => employeeInstanceOperationAlias)
					.JoinAlias(() => employeeInstanceOperationAlias.InventoryNomenclatureInstance, () => instanceAlias)
					.Where(() => employeeInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !instanceAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeInstanceOperationAlias.Employee.Id).IsIn(parameters.EmployeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeInstanceOperationAlias.Employee.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.EntityId)
						.SelectSum(() => employeeInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => instanceAlias.Id).Asc
					.ThenBy(() => employeeInstanceOperationAlias.Employee.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			IQueryOver<CarBulkGoodsAccountingOperation, CarBulkGoodsAccountingOperation> bulkBalanceByCarsQuery = null;
			IQueryOver<CarInstanceGoodsAccountingOperation, CarInstanceGoodsAccountingOperation> instanceBalanceByCarsQuery = null;

			if(parameters.CarsIds.Any())
			{
				bulkBalanceByCarsQuery = localUow.Session.QueryOver(() => carBulkOperationAlias)
					.Where(() => carBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => carBulkOperationAlias.Car.Id).IsIn(parameters.CarsIds)
					.SelectList(list => list
						.SelectGroup(() => carBulkOperationAlias.Car.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => nomAlias.Name).WithAlias(() => resultAlias.EntityName)
						.SelectSum(() => carBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => carBulkOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());

				instanceBalanceByCarsQuery = localUow.Session.QueryOver(() => carInstanceOperationAlias)
					.JoinAlias(() => carInstanceOperationAlias.InventoryNomenclatureInstance, () => instanceAlias)
					.Where(() => carInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !instanceAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => carInstanceOperationAlias.Car.Id).IsIn(parameters.CarsIds)
					.SelectList(list => list
						.SelectGroup(() => carInstanceOperationAlias.Car.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.EntityId)
						.SelectSum(() => carInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => instanceAlias.Id).Asc
					.ThenBy(() => carInstanceOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			#region Минимальный остаток номенклатуры на складе

			NomenclatureMinimumBalanceByWarehouse nomenclatureMinimumBalanceByWarehouseAlias = null;

			var minWarehouseBalanceSubquery = QueryOver.Of(() => nomenclatureMinimumBalanceByWarehouseAlias)
				.Where(() => nomenclatureMinimumBalanceByWarehouseAlias.Nomenclature.Id == nomAlias.Id)
				.Select(Projections.Max(() => nomenclatureMinimumBalanceByWarehouseAlias.MinimumBalance));

			if(parameters.WarehousesIds.Any())
			{
				minWarehouseBalanceSubquery.Where(Restrictions.In(Projections.Property(() => nomenclatureMinimumBalanceByWarehouseAlias.Warehouse.Id), parameters.WarehousesIds));
			};

			NomenclatureMinimumBalanceByWarehouseNode nomenclatureMinimumBalanceByWarehouseNode = null;

			var minStockQuery = localUow.Session.QueryOver(() => nomAlias)				
				.Where(() => !nomAlias.IsArchive)
				.SelectList(list => list
					.SelectGroup(() => nomAlias.Id).WithAlias(() => nomenclatureMinimumBalanceByWarehouseNode.NomenclatureId)
					.Select(Projections.Conditional(
						Restrictions.IsNull(Projections.SubQuery(minWarehouseBalanceSubquery)),
						Projections.Cast(NHibernateUtil.Int32, Projections.Property(() => nomAlias.MinStockCount)),
						Projections.SubQuery(minWarehouseBalanceSubquery)
						)).WithAlias(() => nomenclatureMinimumBalanceByWarehouseNode.MinimumBalance))
				.TransformUsing(Transformers.AliasToBean<NomenclatureMinimumBalanceByWarehouseNode>())
				.OrderBy(() => nomAlias.Id).Asc;

			#endregion

			var instanceDataQuery = localUow.Session.QueryOver(() => nomAlias)
				.JoinEntityAlias(() => instanceAlias, () => nomAlias.Id == instanceAlias.Nomenclature.Id)
				.Where(() => !instanceAlias.IsArchive)
				.SelectList(list => list
					.Select(() => instanceAlias.Id).WithAlias(() => instanceDataAlias.Id)
					.Select(() => instanceAlias.PurchasePrice).WithAlias(() => instanceDataAlias.PurchasePrice)
					.Select(n => n.Name).WithAlias(() => instanceDataAlias.Name)
					.Select(() => instanceAlias.InventoryNumber).WithAlias(() => instanceDataAlias.InventoryNumber)
					.Select(() => instanceAlias.IsUsed).WithAlias(() => instanceDataAlias.IsUsed)
				)
				.TransformUsing(Transformers.AliasToBean<InstanceData>())
				.OrderBy(() => instanceAlias.Id).Asc;

			Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			ReservedBalance reservedBalance = null;
			ProductGroup productGroupAlias = null;

			OrderStatus[] orderStatusesToCalcReservedItems =
				new[] { OrderStatus.Accepted, OrderStatus.InTravelList, OrderStatus.OnLoading };

			var reservedItemsQuery = localUow.Session.QueryOver(() => orderAlias)
				.Where(Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), orderStatusesToCalcReservedItems))
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => nomAlias)
				.JoinAlias(() => nomAlias.ProductGroup, () => productGroupAlias)
				.Where(() => nomAlias.DoNotReserve == false)
				.Where(() => !nomAlias.IsArchive && !nomAlias.IsSerial);

			var nomenclatureQuery = localUow.Session.QueryOver(() => nomAlias);

			if(parameters.NomenclatureTypesSelected)
			{
				var typesIds = parameters.NomenclatureTypes.Select(x => (int)x.Value).ToArray();

				bulkBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				instanceBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				bulkBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				instanceBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				bulkBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				instanceBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				minStockQuery.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				instanceDataQuery.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				reservedItemsQuery.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				nomenclatureQuery.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
			}

			if(parameters.NomenclaturesSelected && !parameters.AllNomenclaturesSelected)
			{
				var nomsIds = parameters.NomenclaturesIds;
				bulkBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				bulkBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				bulkBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				minStockQuery.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				reservedItemsQuery.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				nomenclatureQuery.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
			}

			if(parameters.InstancesSelected && !parameters.AllInstancesSelected)
			{
				var instancesIds = parameters.InstancesIds;
				instanceBalanceByWarehousesQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
				instanceBalanceByEmployeesQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
				instanceBalanceByCarsQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
				instanceDataQuery.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
			}

			if(parameters.NomenclatureGroupsSelected)
			{
				var groupsIds = parameters.NomenclatureGroupsIds;
				bulkBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				instanceBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				bulkBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				instanceBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				bulkBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				instanceBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				minStockQuery.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				instanceDataQuery.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				reservedItemsQuery.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				nomenclatureQuery.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
			}

			reservedItemsQuery
				.SelectList(list => list
					.SelectGroup(() => nomAlias.Id).WithAlias(() => reservedBalance.ItemId)
					.Select(Projections.Sum(() => orderItemsAlias.Count)).WithAlias(() => reservedBalance.ReservedItemsAmount))
				.TransformUsing(Transformers.AliasToBean<ReservedBalance>());

			#endregion

			return await GenerateBalanceSummaryReportAsync(
				localUow,
				createReportWithReserveData,
				withPrices,
				minStockQuery,
				instanceDataQuery,
				reservedItemsQuery,
				nomenclatureQuery,
				bulkBalanceByWarehousesQuery,
				instanceBalanceByWarehousesQuery,
				bulkBalanceByEmployeesQuery,
				instanceBalanceByEmployeesQuery,
				bulkBalanceByCarsQuery,
				instanceBalanceByCarsQuery,
				parameters,
				defaultReport,
				cancellationToken
				);
		}
		
		private async Task<BalanceSummaryReport> GenerateBalanceSummaryReportAsync(
			IUnitOfWork localUow,
			bool createReportWithReserveData,
			bool withPrices,
			IQueryOver minStockQuery,
			IQueryOver instanceDataQuery,
			IQueryOver<Order, Order> reservedItemsQuery,
			IQueryOver<Nomenclature, Nomenclature> nomenclatureQuery,
			IQueryOver bulkBalanceByWarehousesQuery,
			IQueryOver instanceBalanceByWarehousesQuery,
			IQueryOver bulkBalanceByEmployeesQuery,
			IQueryOver instanceBalanceByEmployeesQuery,
			IQueryOver bulkBalanceByCarsQuery,
			IQueryOver instanceBalanceByCarsQuery,
			WarehousesBalanceSummaryReportParameters parameters,
			BalanceSummaryReport report,
			CancellationToken cancellationToken
			)
		{
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkWarehousesResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceWarehousesResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkEmployeesResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceEmployeesResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkCarsResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceCarsResult = null;

			var batch = localUow.Session.CreateQueryBatch()
				.Add<NomenclatureMinimumBalanceByWarehouseNode>(_minStockKey, minStockQuery)
				.Add<InstanceData>(_instanceDataKey, instanceDataQuery);

			#region fillbatchQuery

			List<ReservedBalance> reservedItems = new List<ReservedBalance>();
			if(createReportWithReserveData)
			{
				reservedItems = reservedItemsQuery.List<ReservedBalance>().ToList();
			}

			List<PriceNode> prices = new List<PriceNode>();
			List<PriceNode> alternativePrices = new List<PriceNode>();
			List<PriceNode> purchasePrices = new List<PriceNode>();

			Nomenclature nomAlias = null;
			PriceNode priceResult = null;
			
			var purchasePriceSubquery = QueryOver.Of<NomenclaturePurchasePrice>()
				.Where(x => x.Nomenclature.Id == nomAlias.Id)
				.OrderBy(x => x.StartDate).Desc
				.Select(x => x.PurchasePrice)
				.Take(1);

			var priceSubquery = QueryOver.Of<NomenclaturePrice>()
				.Where(x => x.Nomenclature.Id == nomAlias.Id)
				.OrderBy(x => x.MinCount).Asc
				.Select(x => x.Price)
				.Take(1);

			var alternativePriceSubquery = QueryOver.Of<AlternativeNomenclaturePrice>()
				.Where(x => x.Nomenclature.Id == nomAlias.Id)
				.OrderBy(x => x.MinCount).Asc
				.Select(x => x.Price)
				.Take(1);
			
			if(withPrices)
			{
				prices = nomenclatureQuery.SelectList(list => list
						.Select(() => nomAlias.Id).WithAlias(() => priceResult.NomenclatureId)
						.SelectSubQuery(priceSubquery).WithAlias(() => priceResult.Amount))
					.TransformUsing(Transformers.AliasToBean<PriceNode>())
					.List<PriceNode>()
					.ToList();

				alternativePrices = nomenclatureQuery.SelectList(list => list
						.Select(() => nomAlias.Id).WithAlias(() => priceResult.NomenclatureId)
						.SelectSubQuery(alternativePriceSubquery).WithAlias(() => priceResult.Amount))
					.TransformUsing(Transformers.AliasToBean<PriceNode>())
					.List<PriceNode>()
					.ToList();

				purchasePrices = nomenclatureQuery.SelectList(list => list
						.Select(() => nomAlias.Id).WithAlias(() => priceResult.NomenclatureId)
						.SelectSubQuery(purchasePriceSubquery).WithAlias(() => priceResult.Amount))
					.TransformUsing(Transformers.AliasToBean<PriceNode>())
					.List<PriceNode>()
					.ToList();
			}

			AddQueriesToQueryBatch(
				batch,
				bulkBalanceByWarehousesQuery,
				instanceBalanceByWarehousesQuery,
				bulkBalanceByEmployeesQuery,
				instanceBalanceByEmployeesQuery,
				bulkBalanceByCarsQuery,
				instanceBalanceByCarsQuery);

			#endregion

			#region GetResults

			if(bulkBalanceByWarehousesQuery != null)
			{
				bulkWarehousesResult = GetStoragesBalanceResult(batch, _bulkBalanceWarehousesKey);
			}

			if(instanceBalanceByWarehousesQuery != null)
			{
				instanceWarehousesResult = GetStoragesBalanceResult(batch, _instanceBalanceWarehousesKey);
			}

			if(bulkBalanceByEmployeesQuery != null)
			{
				bulkEmployeesResult = GetStoragesBalanceResult(batch, _bulkBalanceEmployeesKey);
			}

			if(instanceBalanceByEmployeesQuery != null)
			{
				instanceEmployeesResult = GetStoragesBalanceResult(batch, _instanceBalanceEmployeesKey);
			}

			if(bulkBalanceByCarsQuery != null)
			{
				bulkCarsResult = GetStoragesBalanceResult(batch, _bulkBalanceCarsKey);
			}

			if(instanceBalanceByCarsQuery != null)
			{
				instanceCarsResult = GetStoragesBalanceResult(batch, _instanceBalanceCarsKey);
			}

			var minStockResult = batch.GetResult<NomenclatureMinimumBalanceByWarehouseNode>(_minStockKey).ToArray();
			var instanceData = batch.GetResult<InstanceData>(_instanceDataKey).ToArray();

			var counter = 0;

			#endregion

			GenerateBalanceSummaryReportInstanceRows(
				cancellationToken,
				ref counter,
				parameters.Instances,
				instanceData,
				parameters.WarehouseStorages,
				instanceWarehousesResult,
				parameters.EmployeeStorages,
				instanceEmployeesResult,
				parameters.CarStorages,
				instanceCarsResult,
				report,
				withPrices);
			
			GenerateBalanceSummaryReportBulkRows(
				cancellationToken,
				ref counter,
				parameters.Nomenclatures,
				reservedItems,
				prices,
				alternativePrices,
				purchasePrices,
				minStockResult,
				parameters.WarehouseStorages,
				bulkWarehousesResult,
				parameters.EmployeeStorages,
				bulkEmployeesResult,
				parameters.CarStorages,
				bulkCarsResult,
				report);

			RemoveStoragesByFilterCondition(report, cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();
			return await new ValueTask<BalanceSummaryReport>(report);
		}

		private Dictionary<NomenclatureStorageIds, BalanceBean> GetStoragesBalanceResult(IQueryBatch batch, string queryKey)
		{
			return batch.GetResult<BalanceBean>(queryKey)
				.ToDictionary(x => new NomenclatureStorageIds(x.EntityId, x.StorageId));
		}
		
		private void GenerateBalanceSummaryReportInstanceRows(
			CancellationToken cancellationToken,
			ref int counter,
			IList<SelectableParameter> instances,
			InstanceData[] instanceData,
			IList<SelectableParameter> warehouseStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceWarehousesResult,
			IList<SelectableParameter> employeeStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceEmployeesResult,
			IList<SelectableParameter> carStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceCarsResult,
			BalanceSummaryReport report,
			bool withPrices)
		{
			for(var instancesCounter = 0; instancesCounter < instances?.Count; instancesCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var instanceId = (int)instances[instancesCounter].Value;

				var row = new BalanceSummaryRow
				{
					Num = ++counter,
					EntityId = instanceId,
					NomTitle = instanceData[instancesCounter].Name,
					InventoryNumber = instanceData[instancesCounter].GetInventoryNumber,
					WarehousesBalances = new List<decimal>(),
					EmployeesBalances = new List<decimal>(),
					CarsBalances = new List<decimal>()
				};

				if(withPrices)
				{
					row.PurchasePrice = instanceData[instancesCounter].PurchasePrice;
				}

				row.FillStoragesBalance(StorageType.Warehouse, warehouseStorages, instanceId, instanceWarehousesResult, cancellationToken);
				row.FillStoragesBalance(StorageType.Employee, employeeStorages, instanceId, instanceEmployeesResult, cancellationToken);
				row.FillStoragesBalance(StorageType.Car, carStorages, instanceId, instanceCarsResult, cancellationToken);

				AddRow(report, row);
			}
		}

		private void GenerateBalanceSummaryReportBulkRows(
			CancellationToken cancellationToken,
			ref int counter,
			IList<SelectableParameter> noms,
			IList<ReservedBalance> reservedItems,
			IList<PriceNode> prices,
			IList<PriceNode> alternativePrices,
			IList<PriceNode> purchasePrices,
			NomenclatureMinimumBalanceByWarehouseNode[] minStockResult,
			IList<SelectableParameter> warehouseStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkWarehousesResult,
			IList<SelectableParameter> employeeStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkEmployeesResult,
			IList<SelectableParameter> carStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkCarsResult,
			BalanceSummaryReport report)
		{
			for(var nomsCounter = 0; nomsCounter < noms?.Count; nomsCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var nomenclatureId = (int)noms[nomsCounter].Value;

				var row = new BalanceSummaryRow
				{
					Num = ++counter,
					EntityId = nomenclatureId,
					NomTitle = noms[nomsCounter].Title,
					InventoryNumber = "-",
					WarehousesBalances = new List<decimal>(),
					EmployeesBalances = new List<decimal>(),
					CarsBalances = new List<decimal>(),
					ReservedItemsAmount = reservedItems
						.Where(i => i.ItemId == (int)noms[nomsCounter].Value)
						.Select(i => i.ReservedItemsAmount).FirstOrDefault() ?? 0,
					Price = prices.SingleOrDefault(x => x.NomenclatureId == (int)noms[nomsCounter].Value)?.Amount ?? 0,
					AlternativePrice =
						alternativePrices.SingleOrDefault(x => x.NomenclatureId == (int)noms[nomsCounter].Value)?.Amount ?? 0,
					PurchasePrice = purchasePrices.SingleOrDefault(x => x.NomenclatureId == (int)noms[nomsCounter].Value)?.Amount ?? 0,
					Min = minStockResult.FirstOrDefault(x => x.NomenclatureId == nomenclatureId).MinimumBalance
				};

				row.FillStoragesBalance(StorageType.Warehouse, warehouseStorages, nomenclatureId, bulkWarehousesResult, cancellationToken);
				row.FillStoragesBalance(StorageType.Employee, employeeStorages, nomenclatureId, bulkEmployeesResult, cancellationToken);
				row.FillStoragesBalance(StorageType.Car, carStorages, nomenclatureId, bulkCarsResult, cancellationToken);

				AddRow(report, row);
			}
		}
		
		private void RemoveStoragesByFilterCondition(BalanceSummaryReport report, CancellationToken cancellationToken)
		{
			if(AllWarehouses)
			{
				return;
			}

			RemoveWarehousesByFilterCondition(report, cancellationToken);
			RemoveEmployeesByFilterCondition(report, cancellationToken);
			RemoveCarsByFilterCondition(report, cancellationToken);
		}

		private void RemoveWarehousesByFilterCondition(BalanceSummaryReport report, CancellationToken cancellationToken)
		{
			if(report.WarehouseStoragesTitles is null)
			{
				return;
			}
			
			for(var warCounter = 0; warCounter < report.WarehouseStoragesTitles.Count; warCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				if(IsGreaterThanZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.WarehousesBalances[warCounter] > 0) == null
					|| IsLessOrEqualZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.WarehousesBalances[warCounter] <= 0) == null
					|| IsLessThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min < row.WarehousesBalances[warCounter]) == null
					|| IsGreaterOrEqualThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min >= row.WarehousesBalances[warCounter]) == null)
				{
					RemoveStorageByIndex(report, StorageType.Warehouse, ref warCounter, cancellationToken);
				}
			}
		}

		private void RemoveEmployeesByFilterCondition(BalanceSummaryReport report, CancellationToken cancellationToken)
		{
			if(report.EmployeeStoragesTitles is null)
			{
				return;
			}
			
			for(var empCounter = 0; empCounter < report.EmployeeStoragesTitles.Count; empCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				if(IsGreaterThanZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.EmployeesBalances[empCounter] > 0) == null
					|| IsLessOrEqualZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.EmployeesBalances[empCounter] <= 0) == null
					|| IsLessThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min < row.EmployeesBalances[empCounter]) == null
					|| IsGreaterOrEqualThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min >= row.EmployeesBalances[empCounter]) == null)
				{
					RemoveStorageByIndex(report, StorageType.Employee, ref empCounter, cancellationToken);
				}
			}
		}

		private void RemoveCarsByFilterCondition(BalanceSummaryReport report, CancellationToken cancellationToken)
		{
			if(report.CarStoragesTitles is null)
			{
				return;
			}
			
			for(var carCounter = 0; carCounter < report.CarStoragesTitles.Count; carCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				if(IsGreaterThanZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.CarsBalances[carCounter] > 0) == null
					|| IsLessOrEqualZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.CarsBalances[carCounter] <= 0) == null
					|| IsLessThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min < row.CarsBalances[carCounter]) == null
					|| IsGreaterOrEqualThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min >= row.CarsBalances[carCounter]) == null)
				{
					RemoveStorageByIndex(report, StorageType.Car, ref carCounter, cancellationToken);
				}
			}
		}

		private void RemoveStorageByIndex(
			BalanceSummaryReport report,
			StorageType storageType,
			ref int counter,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			switch(storageType)
			{
				case StorageType.Warehouse:
					report.RemoveWarehouseByIndex(counter);
					break;
				case StorageType.Employee:
					report.RemoveEmployeeByIndex(counter);
					break;
				case StorageType.Car:
					report.RemoveCarByIndex(counter);
					break;
			}

			counter--;
		}

		#endregion

		#region GenerateActiveStoragesBalanceSummaryReport

		private async Task<ActiveStoragesBalanceSummaryReport> GenerateActiveStoragesBalanceSummaryReportAsync(
			DateTime endDate,
			IUnitOfWork localUow,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			endDate = endDate.LatestDayTime();

			var parameters = new WarehousesBalanceSummaryReportParameters()
				.AddNomenclaturesSet(_nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNomenclature))
				.AddInstancesSet(_nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterInstances))
				.AddNomenclatureTypesSet(_nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNomenclatureType))
				.AddNomenclatureGroupsSet(_nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterProductGroups))
				.AddWarehouseStorages(_storagesFilter, ParameterWarehouseStorages)
				.AddEmployeeStorages(_storagesFilter, ParameterEmployeeStorages)
				.AddCarStorages(_storagesFilter, ParameterCarStorages);

			Nomenclature nomAlias = null;

			var activeStoragesReport = new ActiveStoragesBalanceSummaryReport
			{
				EndDate = endDate,
				ActiveStoragesBalanceRows = new List<ActiveStoragesBalanceSummaryRow>()
			};

			#region Запросы

			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeBulkGoodsAccountingOperation employeeBulkOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarBulkGoodsAccountingOperation carBulkOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			InventoryNomenclatureInstance instanceAlias = null;
			BalanceBean resultAlias = null;

			IQueryOver<WarehouseBulkGoodsAccountingOperation, WarehouseBulkGoodsAccountingOperation> bulkBalanceByWarehousesQuery = null;
			IQueryOver<WarehouseInstanceGoodsAccountingOperation, WarehouseInstanceGoodsAccountingOperation>
				instanceBalanceByWarehousesQuery = null;

			if(parameters.WarehousesIds != null)
			{
				bulkBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseBulkOperationAlias)
					.Where(() => warehouseBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseBulkOperationAlias.Warehouse.Id).IsIn(parameters.WarehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseBulkOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => nomAlias.Name).WithAlias(() => resultAlias.EntityName)
						.SelectSum(() => warehouseBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.Where(Restrictions.Gt(Projections.Sum(() => warehouseBulkOperationAlias.Amount), 0))
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => warehouseBulkOperationAlias.Warehouse.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());

				instanceBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseInstanceOperationAlias)
					.JoinAlias(() => warehouseInstanceOperationAlias.InventoryNomenclatureInstance, () => instanceAlias)
					.Where(() => warehouseInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !instanceAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseInstanceOperationAlias.Warehouse.Id).IsIn(parameters.WarehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseInstanceOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.EntityId)
						.SelectSum(() => warehouseInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.Where(Restrictions.Gt(Projections.Sum(() => warehouseInstanceOperationAlias.Amount), 0))
					.OrderBy(() => instanceAlias.Id).Asc
					.ThenBy(() => warehouseInstanceOperationAlias.Warehouse.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			IQueryOver<EmployeeBulkGoodsAccountingOperation, EmployeeBulkGoodsAccountingOperation> bulkBalanceByEmployeesQuery = null;
			IQueryOver<EmployeeInstanceGoodsAccountingOperation, EmployeeInstanceGoodsAccountingOperation> instanceBalanceByEmployeesQuery =
				null;

			if(parameters.EmployeesIds != null)
			{
				bulkBalanceByEmployeesQuery = localUow.Session.QueryOver(() => employeeBulkOperationAlias)
					.Where(() => employeeBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeBulkOperationAlias.Employee.Id).IsIn(parameters.EmployeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeBulkOperationAlias.Employee.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => nomAlias.Name).WithAlias(() => resultAlias.EntityName)
						.Select(() => "-").WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => employeeBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.Where(Restrictions.Gt(Projections.Sum(() => employeeBulkOperationAlias.Amount), 0))
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => employeeBulkOperationAlias.Employee.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());

				instanceBalanceByEmployeesQuery = localUow.Session.QueryOver(() => employeeInstanceOperationAlias)
					.JoinAlias(() => employeeInstanceOperationAlias.InventoryNomenclatureInstance, () => instanceAlias)
					.Where(() => employeeInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !instanceAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeInstanceOperationAlias.Employee.Id).IsIn(parameters.EmployeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeInstanceOperationAlias.Employee.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => nomAlias.Name).WithAlias(() => resultAlias.EntityName)
						.Select(InventoryNomenclatureInstanceProjections.InventoryNumberProjection())
							.WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => employeeInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.Where(Restrictions.Gt(Projections.Sum(() => employeeInstanceOperationAlias.Amount), 0))
					.OrderBy(() => instanceAlias.Id).Asc
					.ThenBy(() => employeeInstanceOperationAlias.Employee.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			IQueryOver<CarBulkGoodsAccountingOperation, CarBulkGoodsAccountingOperation> bulkBalanceByCarsQuery = null;
			IQueryOver<CarInstanceGoodsAccountingOperation, CarInstanceGoodsAccountingOperation> instanceBalanceByCarsQuery = null;

			if(parameters.CarsIds != null)
			{
				bulkBalanceByCarsQuery = localUow.Session.QueryOver(() => carBulkOperationAlias)
					.Where(() => carBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => carBulkOperationAlias.Car.Id).IsIn(parameters.CarsIds)
					.SelectList(list => list
						.SelectGroup(() => carBulkOperationAlias.Car.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => nomAlias.Name).WithAlias(() => resultAlias.EntityName)
						.Select(() => "-").WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => carBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.Where(Restrictions.Gt(Projections.Sum(() => carBulkOperationAlias.Amount), 0))
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => carBulkOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());

				instanceBalanceByCarsQuery = localUow.Session.QueryOver(() => carInstanceOperationAlias)
					.JoinAlias(() => carInstanceOperationAlias.InventoryNomenclatureInstance, () => instanceAlias)
					.Where(() => carInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !instanceAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => carInstanceOperationAlias.Car.Id).IsIn(parameters.CarsIds)
					.SelectList(list => list
						.SelectGroup(() => carInstanceOperationAlias.Car.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.EntityId)
						.Select(() => nomAlias.Name).WithAlias(() => resultAlias.EntityName)
						.Select(InventoryNomenclatureInstanceProjections.InventoryNumberProjection())
							.WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => carInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.Where(Restrictions.Gt(Projections.Sum(() => carInstanceOperationAlias.Amount), 0))
					.OrderBy(() => instanceAlias.Id).Asc
					.ThenBy(() => carInstanceOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			var nomenclatureQuery = localUow.Session.QueryOver(() => nomAlias);

			if(parameters.NomenclatureTypesSelected)
			{
				var typesIds = parameters.NomenclatureTypes.Select(x => (int)x.Value).ToArray();
				bulkBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				instanceBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				bulkBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				instanceBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				bulkBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				instanceBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
				nomenclatureQuery.WhereRestrictionOn(() => nomAlias.Category).IsIn(typesIds);
			}

			if(parameters.NomenclaturesSelected && !parameters.AllNomenclaturesSelected)
			{
				var nomsIds = parameters.NomenclaturesIds;
				bulkBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				bulkBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				bulkBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				nomenclatureQuery.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
			}

			if(parameters.InstancesSelected && !parameters.AllInstancesSelected)
			{
				var instancesIds = parameters.InstancesIds;
				instanceBalanceByWarehousesQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
				instanceBalanceByEmployeesQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
				instanceBalanceByCarsQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
			}

			if(parameters.NomenclatureGroupsSelected)
			{
				var groupsIds = parameters.NomenclatureGroupsIds;
				bulkBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				instanceBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				bulkBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				instanceBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				bulkBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				instanceBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
				nomenclatureQuery.WhereRestrictionOn(() => nomAlias.ProductGroup.Id).IsIn(groupsIds);
			}

			#endregion
			
			return await GenerateActiveStoragesBalanceSummaryReportAsync(
				localUow,
				bulkBalanceByWarehousesQuery,
				instanceBalanceByWarehousesQuery,
				bulkBalanceByEmployeesQuery,
				instanceBalanceByEmployeesQuery,
				bulkBalanceByCarsQuery,
				instanceBalanceByCarsQuery,
				parameters,
				activeStoragesReport,
				cancellationToken
				);
		}

		private async Task<ActiveStoragesBalanceSummaryReport> GenerateActiveStoragesBalanceSummaryReportAsync(
			IUnitOfWork localUow,
			IQueryOver bulkBalanceByWarehousesQuery,
			IQueryOver instanceBalanceByWarehousesQuery,
			IQueryOver bulkBalanceByEmployeesQuery,
			IQueryOver instanceBalanceByEmployeesQuery,
			IQueryOver bulkBalanceByCarsQuery,
			IQueryOver instanceBalanceByCarsQuery,
			WarehousesBalanceSummaryReportParameters parameters,
			ActiveStoragesBalanceSummaryReport report,
			CancellationToken cancellationToken)
		{
			ILookup<int, BalanceBean> bulkWarehousesResult = null;
			ILookup<int, BalanceBean> instanceWarehousesResult = null;
			ILookup<int, BalanceBean> bulkEmployeesResult = null;
			ILookup<int, BalanceBean> instanceEmployeesResult = null;
			ILookup<int, BalanceBean> bulkCarsResult = null;
			ILookup<int, BalanceBean> instanceCarsResult = null;

			var batch = localUow.Session.CreateQueryBatch();

			#region fillbatchQuery

			AddQueriesToQueryBatch(
				batch,
				bulkBalanceByWarehousesQuery,
				instanceBalanceByWarehousesQuery,
				bulkBalanceByEmployeesQuery,
				instanceBalanceByEmployeesQuery,
				bulkBalanceByCarsQuery,
				instanceBalanceByCarsQuery);

			#endregion

			#region GetResults

			if(bulkBalanceByWarehousesQuery != null)
			{
				bulkWarehousesResult = GetActiveStoragesBalanceResult(batch, _bulkBalanceWarehousesKey);
			}

			if(instanceBalanceByWarehousesQuery != null)
			{
				instanceWarehousesResult = GetActiveStoragesBalanceResult(batch, _instanceBalanceWarehousesKey);
			}

			if(bulkBalanceByEmployeesQuery != null)
			{
				bulkEmployeesResult = GetActiveStoragesBalanceResult(batch, _bulkBalanceEmployeesKey);
			}

			if(instanceBalanceByEmployeesQuery != null)
			{
				instanceEmployeesResult = GetActiveStoragesBalanceResult(batch, _instanceBalanceEmployeesKey);
			}

			if(bulkBalanceByCarsQuery != null)
			{
				bulkCarsResult = GetActiveStoragesBalanceResult(batch, _bulkBalanceCarsKey);
			}

			if(instanceBalanceByCarsQuery != null)
			{
				instanceCarsResult = GetActiveStoragesBalanceResult(batch, _instanceBalanceCarsKey);
			}

			#endregion

			GenerateActiveStoragesBalanceSummaryRows(
				cancellationToken,
				parameters.WarehouseStorages,
				bulkWarehousesResult,
				instanceWarehousesResult,
				parameters.EmployeeStorages,
				bulkEmployeesResult,
				instanceEmployeesResult,
				parameters.CarStorages,
				bulkCarsResult,
				instanceCarsResult,
				report);

			cancellationToken.ThrowIfCancellationRequested();
			return await new ValueTask<ActiveStoragesBalanceSummaryReport>(report);
		}

		private void GenerateActiveStoragesBalanceSummaryRows(
			CancellationToken cancellationToken,
			IList<SelectableParameter> warehouseStorages,
			ILookup<int, BalanceBean> bulkWarehousesResult,
			ILookup<int, BalanceBean> instanceWarehousesResult,
			IList<SelectableParameter> employeeStorages,
			ILookup<int, BalanceBean> bulkEmployeesResult,
			ILookup<int, BalanceBean> instanceEmployeesResult,
			IList<SelectableParameter> carStorages,
			ILookup<int, BalanceBean> bulkCarsResult,
			ILookup<int, BalanceBean> instanceCarsResult,
			ActiveStoragesBalanceSummaryReport report)
		{
			cancellationToken.ThrowIfCancellationRequested();
			
			FillActiveStoragesBalances(warehouseStorages, bulkWarehousesResult, instanceWarehousesResult, report, cancellationToken);
			FillActiveStoragesBalances(employeeStorages, bulkEmployeesResult, instanceEmployeesResult, report, cancellationToken);
			FillActiveStoragesBalances(carStorages, bulkCarsResult, instanceCarsResult, report, cancellationToken);
		}

		private void FillActiveStoragesBalances(
			IList<SelectableParameter> storages,
			ILookup<int, BalanceBean> bulksResult,
			ILookup<int, BalanceBean> instancesResult,
			ActiveStoragesBalanceSummaryReport report,
			CancellationToken cancellationToken)
		{
			for(var i = 0; i < storages?.Count; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				var storageKey = (int)storages[i].Value;
				var instancesBalances = instancesResult[storageKey];
				var nomenclaturesBalances = bulksResult[storageKey];

				if(!instancesBalances.Any() && !nomenclaturesBalances.Any())
				{
					continue;
				}

				var rowNum = 0;

				GenerateActiveStoragesBalanceInstanceRows(storages, report, instancesBalances, ref rowNum, i, cancellationToken);
				GenerateActiveStoragesBalanceBulkRows(storages, report, nomenclaturesBalances, rowNum, i, cancellationToken);
			}
		}

		private void GenerateActiveStoragesBalanceBulkRows(
			IList<SelectableParameter> storages,
			ActiveStoragesBalanceSummaryReport report,
			IEnumerable<BalanceBean> nomenclaturesBalances,
			int rowNum,
			int i,
			CancellationToken cancellationToken)
		{
			foreach(var balanceBean in nomenclaturesBalances)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				var row = new ActiveStoragesBalanceSummaryRow
				{
					StorageId = balanceBean.StorageId,
					EntityId = balanceBean.EntityId,
					Entity = balanceBean.EntityName,
					InventoryNumber = balanceBean.InventoryNumber,
					Balance = balanceBean.Amount,
					RowNumberFromStorage = ++rowNum
				};
				
				UpdateStorageTitle(row, storages, rowNum, i);
				report.ActiveStoragesBalanceRows.Add(row);
			}
		}

		private void GenerateActiveStoragesBalanceInstanceRows(
			IList<SelectableParameter> storages,
			ActiveStoragesBalanceSummaryReport report,
			IEnumerable<BalanceBean> instancesBalances,
			ref int rowNum,
			int i,
			CancellationToken cancellationToken)
		{
			foreach(var balanceBean in instancesBalances)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				var row = new ActiveStoragesBalanceSummaryRow
				{
					StorageId = balanceBean.StorageId,
					EntityId = balanceBean.EntityId,
					Entity = balanceBean.EntityName,
					InventoryNumber = balanceBean.InventoryNumber,
					Balance = balanceBean.Amount,
					RowNumberFromStorage = ++rowNum
				};

				UpdateStorageTitle(row, storages, rowNum, i);
				report.ActiveStoragesBalanceRows.Add(row);
			}
		}

		private ILookup<int, BalanceBean> GetActiveStoragesBalanceResult(IQueryBatch batch, string queryKey)
		{
			return batch.GetResult<BalanceBean>(queryKey)
				.ToLookup(x => x.StorageId);
		}
		
		private void UpdateStorageTitle(
			ActiveStoragesBalanceSummaryRow row,
			IList<SelectableParameter> storages,
			int rowNum,
			int i)
		{
			if(rowNum == _firstRow)
			{
				row.Storage = storages[i].Title;
				row.RowNumberStorage = i + 1;
			}
		}

		#endregion
		
		private void UpdateGroupingActiveStorageState()
		{
			OnPropertyChanged(nameof(Sensitivity));
			ShowReserve = false;
			ShowPrices = false;
			EndDate = DateTime.Today.AddDays(1);
			
			if(_groupingActiveStorage)
			{
				UpdateStateForGroupingActiveStorage();
			}
			else
			{
				UpdateStorageParameters();
				UpdateActiveSelectedStorageType(null);
			}
		}

		private void UpdateStateForGroupingActiveStorage()
		{
			IsGreaterThanZeroByNomenclature = _groupingActiveStorage;
			IsGreaterThanZeroByWarehouse = _groupingActiveStorage;

			if(StoragesViewModel.CurrentParameterSet is null)
			{
				StoragesViewModel.CurrentParameterSet = StoragesViewModel.ReportFilter.ParameterSets.First();
			}
			else
			{
				UpdateStorageParameters();
			}
			
			if(!string.IsNullOrWhiteSpace(StoragesViewModel.CurrentParameterSet.SearchValue))
			{
				StoragesViewModel.SearchValue = string.Empty;
			}
			
			StoragesViewModel.CurrentParameterSet.SelectAll();
			UpdateActiveSelectedStorageType(StoragesViewModel.CurrentParameterSet);
		}

		private void UpdateActiveSelectedStorageType(SelectableParameterSet currentActiveParameterSet)
		{
			if(currentActiveParameterSet is null)
			{
				ActiveSelectedStorageType = null;
				return;
			}
			
			switch(currentActiveParameterSet.ParameterName)
			{
				case ParameterEmployeeStorages:
					ActiveSelectedStorageType = StorageType.Employee;
					break;
				case ParameterCarStorages:
					ActiveSelectedStorageType = StorageType.Car;
					break;
				default:
					ActiveSelectedStorageType = null;
					break;
			}
		}
		
		private void AddQueriesToQueryBatch(
			IQueryBatch batch,
			IQueryOver bulkBalanceByWarehousesQuery,
			IQueryOver instanceBalanceByWarehousesQuery,
			IQueryOver bulkBalanceByEmployeesQuery,
			IQueryOver instanceBalanceByEmployeesQuery,
			IQueryOver bulkBalanceByCarsQuery,
			IQueryOver instanceBalanceByCarsQuery)
		{
			if(bulkBalanceByWarehousesQuery != null)
			{
				batch.Add<BalanceBean>(_bulkBalanceWarehousesKey, bulkBalanceByWarehousesQuery);
			}

			if(instanceBalanceByWarehousesQuery != null)
			{
				batch.Add<BalanceBean>(_instanceBalanceWarehousesKey, instanceBalanceByWarehousesQuery);
			}

			if(bulkBalanceByEmployeesQuery != null)
			{
				batch.Add<BalanceBean>(_bulkBalanceEmployeesKey, bulkBalanceByEmployeesQuery);
			}

			if(instanceBalanceByEmployeesQuery != null)
			{
				batch.Add<BalanceBean>(_instanceBalanceEmployeesKey, instanceBalanceByEmployeesQuery);
			}

			if(bulkBalanceByCarsQuery != null)
			{
				batch.Add<BalanceBean>(_bulkBalanceCarsKey, bulkBalanceByCarsQuery);
			}

			if(instanceBalanceByCarsQuery != null)
			{
				batch.Add<BalanceBean>(_instanceBalanceCarsKey, instanceBalanceByCarsQuery);
			}
		}

		#region Export Report

		public void ExportReport()
		{
			using(var wb = new XLWorkbook())
			{
				var sheetName = $"{DateTime.Now:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				if(GroupingActiveStorage)
				{
					InsertActiveStoragesBalanceSummaryReportValues(ws);
				}
				else
				{
					InsertBalanceSummaryReportValues(ws, _isCreatedWithReserveData);
				}
				
				ws.Columns().AdjustToContents();

				if(TryGetSavePath(out string path))
				{
					wb.SaveAs(path);
				}
			}
		}

		#region BalanceSummaryReport

		private void InsertBalanceSummaryReportValues(IXLWorksheet ws, bool withReserveData)
		{
			var colNames = GetBalanceSummaryReportColumnTitles(withReserveData);
			var rows = GetBalanceSummaryReportRowsData(withReserveData);
			
			var index = 1;
			foreach(var name in colNames)
			{
				ws.Cell(1, index).Value = name;
				index++;
			}

			ws.Cell(2, 1).InsertData(rows);
			AddStoragesColumns(ws, index);
		}
		
		private static string[] GetBalanceSummaryReportColumnTitles(bool withReserveData)
		{
			if(!withReserveData)
			{
				return new[]
				{
					"№",
					"Код",
					"Наименование",
					"Инвентарный номер",
					"Мин. Остаток",
					"Общий остаток",
					"Разница",
					"Цена закупки",
					"Цена",
					"Цена Kuler Sale"
				};
			}
			
			return new[]
			{
				"№",
				"Код",
				"Наименование",
				"Инвентарный номер",
				"Мин. Остаток",
				"В резерве",
				"Доступно для заказа",
				"Общий остаток",
				"Разница",
				"Цена закупки",
				"Цена",
				"Цена Kuler Sale"
			};
		}
		
		private IEnumerable GetBalanceSummaryReportRowsData(bool withReserveData)
		{
			if(!withReserveData)
			{
				return from row in BalanceSummaryReport.SummaryRows
					select new
					{
						row.Num,
						row.EntityId,
						row.NomTitle,
						row.InventoryNumber,
						row.Min,
						row.Common,
						row.Diff,
						row.PurchasePrice,
						row.Price,
						row.AlternativePrice
					};
			}
			
			return from row in BalanceSummaryReport.SummaryRows
				select new
				{
					row.Num,
					row.EntityId,
					row.NomTitle,
					row.InventoryNumber,
					row.Min,
					row.ReservedItemsAmount,
					row.AvailableItemsAmount,
					row.Common,
					row.Diff,
					row.PurchasePrice,
					row.Price,
					row.AlternativePrice
				};
		}

		private void AddStoragesColumns(IXLWorksheet ws, int index)
		{
			AddWarehouseColumns(ws, ref index);
			AddEmployeeColumns(ws, ref index);
			AddCarColumns(ws, ref index);
		}

		private void AddWarehouseColumns(IXLWorksheet ws, ref int startIndex)
		{
			for(var i = 0; i < BalanceSummaryReport.WarehouseStoragesTitles?.Count; i++)
			{
				ws.Cell(1, startIndex).Value = $"{BalanceSummaryReport.WarehouseStoragesTitles[i]}";
				ws.Cell(2, startIndex).InsertData(BalanceSummaryReport.SummaryRows.Select(sr => sr.WarehousesBalances[i]));
				startIndex++;
			}
		}
		
		private void AddEmployeeColumns(IXLWorksheet ws, ref int startIndex)
		{
			for(var i = 0; i < BalanceSummaryReport.EmployeeStoragesTitles?.Count; i++)
			{
				ws.Cell(1, startIndex).Value = $"{BalanceSummaryReport.EmployeeStoragesTitles[i]}";
				ws.Cell(2, startIndex).InsertData(BalanceSummaryReport.SummaryRows.Select(sr => sr.EmployeesBalances[i]));
				startIndex++;
			}
		}
		
		private void AddCarColumns(IXLWorksheet ws, ref int startIndex)
		{
			for(var i = 0; i < BalanceSummaryReport.CarStoragesTitles?.Count; i++)
			{
				ws.Cell(1, startIndex).Value = $"{BalanceSummaryReport.CarStoragesTitles[i]}";
				ws.Cell(2, startIndex).InsertData(BalanceSummaryReport.SummaryRows.Select(sr => sr.CarsBalances[i]));
				startIndex++;
			}
		}

		#endregion

		#region ActiveStoragesBalanceSummaryReport

		public string GetActiveSelectedStorageTypeTitle() =>
			ActiveSelectedStorageType == null ? _storage : ActiveSelectedStorageType.GetEnumTitle();

		private void InsertActiveStoragesBalanceSummaryReportValues(IXLWorksheet ws)
		{
			var colNames = GetActiveStoragesBalanceSummaryReportColumnTitles();
			var rows = GetActiveStoragesBalanceSummaryReportRowsData();
			
			var index = 1;
			foreach(var name in colNames)
			{
				ws.Cell(1, index).Value = name;
				index++;
			}

			ws.Cell(2, 1).InsertData(rows);
		}

		private string[] GetActiveStoragesBalanceSummaryReportColumnTitles()
		{
			return new[]
			{
				RowNumberTitle,
				GetActiveSelectedStorageTypeTitle(),
				RowNumberTitle,
				IdTitle,
				EntityTitle,
				InventoryNumberTitle.Replace('\n', ' '),
				BalanceTitle
			};
		}

		private IEnumerable GetActiveStoragesBalanceSummaryReportRowsData()
		{
			return from row in ActiveStoragesBalanceSummaryReport.ActiveStoragesBalanceRows
				select new
				{
					row.RowNumberStorage,
					row.Storage,
					row.RowNumberFromStorage,
					row.EntityId,
					row.Entity,
					row.InventoryNumber,
					row.Balance
				};
		}

		#endregion
		
		private bool TryGetSavePath(out string path)
		{
			var extension = ".xlsx";
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}{extension}"
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter(_xlsxFileFilter, $"*{extension}"));
			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			path = result.Path;

			return result.Successful;
		}

		#endregion

		private void UpdateStorageParameters()
		{
			StoragesViewModel.CurrentParameterSet.UpdateParameters();
			StoragesViewModel.CurrentParameterSet = null;
			StoragesViewModel.CurrentParameterSet = StoragesViewModel.ReportFilter.ParameterSets.First();
		}

		private void AddRow(BalanceSummaryReport report, BalanceSummaryRow row)
		{
			if(AllNomenclatures
				|| (IsGreaterThanZeroByNomenclature && row.HasGreaterThanZeroBalance)
				|| (IsLessOrEqualZeroByNomenclature && row.HasLessOrEqualZeroBalance)
				|| (IsLessThanMinByNomenclature && row.HasLessThanMinBalance)
				|| (IsGreaterOrEqualThanMinByNomenclature && row.HasGreaterOrEqualThanMinBalance))
			{
				report.SummaryRows.Add(row);
			}
		}

		#region Настройка фильтров

		private SelectableParameterReportFilterViewModel CreateNomsViewModel()
		{
			_nomsFilter = new SelectableParametersReportFilter(UoW);
			var nomenclatureTypeParam = _nomsFilter.CreateParameterSet("Типы номенклатур",
				_parameterNomenclatureType,
				new ParametersEnumFactory<NomenclatureCategory>());

			var nomenclatureParam = _nomsFilter.CreateParameterSet("Номенклатуры",
				_parameterNomenclature,
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = UoW.Session.QueryOver<Nomenclature>()
						.Where(x => !x.IsArchive)
						.And(x => !x.HasInventoryAccounting);
					if(filters != null && EnumerableExtensions.Any(filters))
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
								query.Where(filterCriterion);
							}
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.OfficialName).WithAlias(() => resultAlias.EntityTitle)
						)
						.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Nomenclature>>())
						.OrderBy(x => x.Id).Asc();
					
					return query.List<SelectableParameter>();
				})
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(
				nomenclatureTypeParam,
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues().ToArray();
					return !EnumerableExtensions.Any(selectedValues)
						? null
						: nomenclatureTypeParam.FilterType == SelectableFilterType.Include
							? Restrictions.On<Nomenclature>(x => x.Category).IsIn(selectedValues)
							: Restrictions.On<Nomenclature>(x => x.Category).Not.IsIn(selectedValues);
				}
			);

			Nomenclature nomenclatureAlias = null;

			var instancesParam = _nomsFilter.CreateParameterSet("Экземпляры",
				_parameterInstances,
				new ParametersFactory(UoW, (filters) =>
				{
					InventoryNomenclatureInstance instanceAlias = null;
					SelectableEntityParameter<InventoryNomenclatureInstance> resultAlias = null;
					
					var query = UoW.Session.QueryOver(() => instanceAlias)
						.JoinAlias(i => i.Nomenclature, () => nomenclatureAlias)
						.Where(i => !i.IsArchive);
					
					if(filters != null && EnumerableExtensions.Any(filters))
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
								query.Where(filterCriterion);
							}
						}
					}
					
					var customName = CustomProjections.Concat(
						Projections.Property(() => nomenclatureAlias.OfficialName),
						Projections.Constant(" "),
						Projections.Property(() => instanceAlias.InventoryNumber));

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(customName).WithAlias(() => resultAlias.EntityTitle)
					).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<InventoryNomenclatureInstance>>());
					return query.List<SelectableParameter>();
				})
			);
			
			instancesParam.AddFilterOnSourceSelectionChanged(
				nomenclatureTypeParam,
				() =>
				{
					var selectedTypes = nomenclatureTypeParam.GetSelectedValues().ToArray();
					if(!selectedTypes.Any())
					{
						return null;
					}
					
					return nomenclatureTypeParam.FilterType == SelectableFilterType.Include
						? Restrictions.On(() => nomenclatureAlias.Category).IsIn(selectedTypes)
						: Restrictions.On(() => nomenclatureAlias.Category).Not.IsIn(selectedTypes);
				});

			ProductGroup productGroupChildAlias = null;
			UoW.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs,
					() => productGroupChildAlias,
					() => !productGroupChildAlias.IsArchive)
				.Fetch(SelectMode.Fetch, () => productGroupChildAlias)
				.List();

			var productGroupsParam = _nomsFilter.CreateParameterSet("Группы товаров",
				_parameterProductGroups,
				new RecursiveParametersFactory<ProductGroup>(UoW, (filters) =>
					{
						var query = UoW.Session.QueryOver<ProductGroup>()
							.Where(p => p.Parent == null)
							.And(p => !p.IsArchive);

						if(filters != null && EnumerableExtensions.Any(filters))
						{
							foreach(var f in filters)
							{
								query.Where(f());
							}
						}

						return query.List();
					},
					x => x.Name,
					x => x.Childs)
			);
			
			nomenclatureParam.AddFilterOnSourceSelectionChanged(
				productGroupsParam,
				() =>
				{
					var selectedGroups = productGroupsParam.GetSelectedValues().ToArray();
					return !EnumerableExtensions.Any(selectedGroups)
						? null
						: productGroupsParam.FilterType == SelectableFilterType.Include
							? Restrictions.On<Nomenclature>(x => x.ProductGroup.Id).IsIn(selectedGroups)
							: Restrictions.On<Nomenclature>(x => x.ProductGroup.Id).Not.IsIn(selectedGroups);
				}
			);
			
			instancesParam.AddFilterOnSourceSelectionChanged(
				productGroupsParam,
				() =>
				{
					var selectedGroups = productGroupsParam.GetSelectedValues().ToArray();
					if(!selectedGroups.Any())
					{
						return null;
					}
					
					return productGroupsParam.FilterType == SelectableFilterType.Include
						? Restrictions.On(() => nomenclatureAlias.ProductGroup.Id).IsIn(selectedGroups)
						: Restrictions.On(() => nomenclatureAlias.ProductGroup.Id).Not.IsIn(selectedGroups);
				});

			return new SelectableParameterReportFilterViewModel(_nomsFilter);
		}

		private SelectableParameterReportFilterViewModel CreateStoragesViewModel()
		{
			_storagesFilter = new SelectableParametersReportFilter(UoW);

			StoragesParametersSets.Add(_storagesFilter.CreateParameterSet(
				"Склады",
				ParameterWarehouseStorages,
				new ParametersFactory(
					UoW,
					filters =>
					{
						SelectableEntityParameter<Warehouse> resultAlias = null;
						var query = UoW.Session.QueryOver<Warehouse>().Where(x => !x.IsArchive);
						if(filters != null && EnumerableExtensions.Any(filters))
						{
							foreach(var f in filters)
							{
								var filterCriterion = f();
								if(filterCriterion != null)
								{
									query.Where(filterCriterion);
								}
							}
						}

						query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
						).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Warehouse>>());
						return query.List<SelectableParameter>();
					})));

			StoragesParametersSets.Add(_storagesFilter.CreateParameterSet(
				"Сотрудники",
				ParameterEmployeeStorages,
				new ParametersFactory(
					UoW,
					filters =>
					{
						SelectableEntityParameter<Employee> resultAlias = null;
						var query = UoW.Session.QueryOver<Employee>();

						if(GroupingActiveStorage)
						{
							query.Where(e => e.Status == EmployeeStatus.IsWorking);
						}
						
						var employeeName = EmployeeProjections.GetEmployeeFullNameProjection();

						if(filters != null && EnumerableExtensions.Any(filters))
						{
							foreach(var f in filters)
							{
								var filterCriterion = f();
								if(filterCriterion != null)
								{
									query.Where(filterCriterion);
								}
							}
						}

						query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(employeeName).WithAlias(() => resultAlias.EntityTitle));

						if(GroupingActiveStorage)
						{
							query.OrderBy(e => e.Category).Asc()
								.ThenBy(employeeName).Asc();
						}
						
						query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
						return query.List<SelectableParameter>();
					})));

			StoragesParametersSets.Add(_storagesFilter.CreateParameterSet(
				"Автомобили",
				ParameterCarStorages,
				new ParametersFactory(
					UoW,
					filters =>
					{
						SelectableEntityParameter<Car> resultAlias = null;
						Car carAlias = null;
						CarModel carModelAlias = null;
						CarVersion carVersionAlias = null;

						var query = UoW.Session.QueryOver(() => carAlias)
							.JoinAlias(c => c.CarModel, () => carModelAlias)
							.Where(x => !x.IsArchive);

						var customName = CustomProjections.Concat(
							Projections.Property(() => carModelAlias.Name),
							Projections.Constant(" ("),
							Projections.Property(() => carAlias.RegistrationNumber),
							Projections.Constant(")"));
						
						var carTypeProjection = Projections.Conditional(
								Restrictions.Where(() => carModelAlias.CarTypeOfUse == CarTypeOfUse.Largus),
								Projections.Constant(0),
								Projections.Conditional(
									Restrictions.Where(() => carModelAlias.CarTypeOfUse == CarTypeOfUse.Minivan),
									Projections.Constant(1),
									Projections.Conditional(
										Restrictions.Where(() => carModelAlias.CarTypeOfUse == CarTypeOfUse.GAZelle),
										Projections.Constant(2),
										Projections.Conditional(
											Restrictions.Where(() => carModelAlias.CarTypeOfUse == CarTypeOfUse.Truck),
											Projections.Constant(3),
											Projections.Constant(4)
											)
										)
									)
								);

						if(GroupingActiveStorage)
						{
							query.JoinAlias(() => carAlias.CarVersions, () => carVersionAlias)
								.Where(() =>
									carVersionAlias.StartDate <= EndDate
									&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate > EndDate))
								.AndRestrictionOn(() => carVersionAlias.CarOwnType).IsIn(new[]{ CarOwnType.Company, CarOwnType.Raskat });
						}

						if(filters != null && EnumerableExtensions.Any(filters))
						{
							foreach(var f in filters)
							{
								var filterCriterion = f();
								if(filterCriterion != null)
								{
									query.Where(filterCriterion);
								}
							}
						}

						query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(customName).WithAlias(() => resultAlias.EntityTitle));

						if(GroupingActiveStorage)
						{
							query.OrderBy(carTypeProjection).Asc()
								.ThenBy(() => carAlias.Id).Asc();
						}
						
						query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Car>>());
						
						return query.List<SelectableParameter>();
					})));

			return new SelectableParameterReportFilterViewModel(_storagesFilter);
		}

		#endregion
	}
}

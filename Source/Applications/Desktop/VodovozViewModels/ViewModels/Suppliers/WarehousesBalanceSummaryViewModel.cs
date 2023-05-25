using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
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
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class WarehousesBalanceSummaryViewModel : DialogTabViewModelBase
	{
		public const string ParameterWarehouseStorages = nameof(Warehouse);
		public const string ParameterEmployeeStorages = nameof(Employee);
		public const string ParameterCarStorages = nameof(Car);
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

		private bool _isGenerating = false;
		private BalanceSummaryReport _report;
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

		public DateTime? EndDate { get; set; } = DateTime.Today;
		public bool ShowReserve { get; set; }
		public bool ShowPrices { get; set; }

		public bool AllNomenclatures { get; set; } = true;
		public bool IsGreaterThanZeroByNomenclature { get; set; }
		public bool IsLessOrEqualZeroByNomenclature { get; set; }
		public bool IsLessThanMinByNomenclature { get; set; }
		public bool IsGreaterOrEqualThanMinByNomenclature { get; set; }

		public bool AllWarehouses { get; set; } = true;
		public bool IsGreaterThanZeroByWarehouse { get; set; }
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

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public BalanceSummaryReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

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
				return await GenerateAsync(EndDate ?? DateTime.Today, ShowReserve, ShowPrices, uow, cancellationToken);
			}
			finally
			{
				uow.Dispose();
			}
		}

		private async Task<BalanceSummaryReport> GenerateAsync(
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

			endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);

			var nomsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNomenclature);
			var noms = nomsSet?.GetIncludedParameters()?.ToList();
			var instancesSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterInstances);
			var instances = instancesSet?.GetIncludedParameters()?.ToList();
			var typesSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNomenclatureType);
			var types = typesSet?.GetIncludedParameters()?.ToList();
			var groupsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterProductGroups);
			var groups = groupsSet?.GetIncludedParameters()?.ToList();
			var warehouseStorages = _storagesFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == ParameterWarehouseStorages)?.GetIncludedParameters()?.ToList();
			var employeeStorages = _storagesFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == ParameterEmployeeStorages)?.GetIncludedParameters()?.ToList();
			var carStorages = _storagesFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == ParameterCarStorages)?.GetIncludedParameters()?.ToList();

			Nomenclature nomAlias = null;

			var warehousesIds = warehouseStorages?.Select(x => (int)x.Value).ToArray();
			var employeesIds = employeeStorages?.Select(x => (int)x.Value).ToArray();
			var carsIds = carStorages?.Select(x => (int)x.Value).ToArray();
			var groupsIds = groups?.Select(x => (int)x.Value).ToArray();
			var groupsSelected = groups?.Any() ?? false;
			var typesSelected = types?.Any() ?? false;
			var nomsSelected = noms?.Any() ?? false;
			var instancesSelected = instances?.Any() ?? false;
			var allNomsSelected = noms?.Count == nomsSet?.Parameters.Count;
			var allInstancesSelected = instances?.Count == instancesSet?.Parameters.Count;

			if(!nomsSelected && !instancesSelected)
			{
				noms?.AddRange(nomsSet.Parameters);
				instances?.AddRange(instancesSet.Parameters);
			}

			var nomsIds = noms?.Select(x => (int)x.Value).ToArray();
			var instancesIds = instances?.Select(x => (int)x.Value).ToArray();

			var report = new BalanceSummaryReport
			{
				EndDate = endDate,
				WarehouseStoragesTitles = warehouseStorages?.Select(x => x.Title).ToList(),
				EmployeeStoragesTitles = employeeStorages?.Select(x => x.Title).ToList(),
				CarStoragesTitles = carStorages?.Select(x => x.Title).ToList(),
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

			if(warehousesIds != null)
			{
				bulkBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseBulkOperationAlias)
					.Where(() => warehouseBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseBulkOperationAlias.Warehouse.Id).IsIn(warehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseBulkOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
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
					.WhereRestrictionOn(() => warehouseInstanceOperationAlias.Warehouse.Id).IsIn(warehousesIds)
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

			if(employeesIds != null)
			{
				bulkBalanceByEmployeesQuery = localUow.Session.QueryOver(() => employeeBulkOperationAlias)
					.Where(() => employeeBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeBulkOperationAlias.Employee.Id).IsIn(employeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeBulkOperationAlias.Employee.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
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
					.WhereRestrictionOn(() => employeeInstanceOperationAlias.Employee.Id).IsIn(employeesIds)
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

			if(carsIds != null)
			{
				bulkBalanceByCarsQuery = localUow.Session.QueryOver(() => carBulkOperationAlias)
					.Where(() => carBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => carBulkOperationAlias.Car.Id).IsIn(carsIds)
					.SelectList(list => list
						.SelectGroup(() => carBulkOperationAlias.Car.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.EntityId)
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
					.WhereRestrictionOn(() => carInstanceOperationAlias.Car.Id).IsIn(carsIds)
					.SelectList(list => list
						.SelectGroup(() => carInstanceOperationAlias.Car.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.EntityId)
						.SelectSum(() => carInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => instanceAlias.Id).Asc
					.ThenBy(() => carInstanceOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			var minStockQuery = localUow.Session.QueryOver(() => nomAlias)
				.Where(() => !nomAlias.IsArchive)
				.Select(n => n.MinStockCount)
				.OrderBy(n => n.Id).Asc;
			
			var instanceDataQuery = localUow.Session.QueryOver(() => nomAlias)
				.JoinEntityAlias(() => instanceAlias, () => nomAlias.Id == instanceAlias.Nomenclature.Id)
				.Where(() => !instanceAlias.IsArchive)
				.SelectList(list => list
					.Select(() => instanceAlias.PurchasePrice).WithAlias(() => instanceDataAlias.PurchasePrice)
					.Select(n => n.Name).WithAlias(() => instanceDataAlias.Name)
					.Select(() => instanceAlias.InventoryNumber).WithAlias(() => instanceDataAlias.InventoryNumber))
				.TransformUsing(Transformers.AliasToBean<InstanceData>())
				.OrderBy(() => instanceAlias.Id).Asc;

			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			ReservedBalance reservedBalance = null;
			ProductGroup productGroupAlias = null;
			PriceNode priceResult = null;

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

			if(typesSelected)
			{
				var typesIds = types.Select(x => (int)x.Value).ToArray();

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

			if(nomsSelected && !allNomsSelected)
			{
				bulkBalanceByWarehousesQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				bulkBalanceByEmployeesQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				bulkBalanceByCarsQuery?.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				minStockQuery.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				reservedItemsQuery.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
				nomenclatureQuery.WhereRestrictionOn(() => nomAlias.Id).IsIn(nomsIds);
			}

			if(instancesSelected && !allInstancesSelected)
			{
				instanceBalanceByWarehousesQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
				instanceBalanceByEmployeesQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
				instanceBalanceByCarsQuery?.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
				instanceDataQuery.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instancesIds);
			}

			if(groupsSelected)
			{
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

			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkWarehousesResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceWarehousesResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkEmployeesResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceEmployeesResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkCarsResult = null;
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceCarsResult = null;

			var batch = localUow.Session.CreateQueryBatch()
				.Add<decimal>(_minStockKey, minStockQuery)
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

			#endregion

			#region GetResults

			if(bulkBalanceByWarehousesQuery != null)
			{
				bulkWarehousesResult =
					batch.GetResult<BalanceBean>(_bulkBalanceWarehousesKey)
						.ToDictionary(x => new NomenclatureStorageIds(x.EntityId, x.StorageId));
			}

			if(instanceBalanceByWarehousesQuery != null)
			{
				instanceWarehousesResult =
					batch.GetResult<BalanceBean>(_instanceBalanceWarehousesKey)
						.ToDictionary(x => new NomenclatureStorageIds(x.EntityId, x.StorageId));
			}

			if(bulkBalanceByEmployeesQuery != null)
			{
				bulkEmployeesResult =
					batch.GetResult<BalanceBean>(_bulkBalanceEmployeesKey)
						.ToDictionary(x => new NomenclatureStorageIds(x.EntityId, x.StorageId));
			}

			if(instanceBalanceByEmployeesQuery != null)
			{
				instanceEmployeesResult =
					batch.GetResult<BalanceBean>(_instanceBalanceEmployeesKey)
						.ToDictionary(x => new NomenclatureStorageIds(x.EntityId, x.StorageId));
			}

			if(bulkBalanceByCarsQuery != null)
			{
				bulkCarsResult =
					batch.GetResult<BalanceBean>(_bulkBalanceCarsKey)
						.ToDictionary(x => new NomenclatureStorageIds(x.EntityId, x.StorageId));
			}

			if(instanceBalanceByCarsQuery != null)
			{
				instanceCarsResult =
					batch.GetResult<BalanceBean>(_instanceBalanceCarsKey)
						.ToDictionary(x => new NomenclatureStorageIds(x.EntityId, x.StorageId));
			}

			var minStockResult = batch.GetResult<decimal>(_minStockKey).ToArray();
			var instanceData = batch.GetResult<InstanceData>(_instanceDataKey).ToArray();

			var counter = 0;

			#endregion

			CreateInstanceRows(
				cancellationToken,
				ref counter,
				instances,
				instanceData,
				warehouseStorages,
				instanceWarehousesResult,
				employeeStorages,
				instanceEmployeesResult,
				carStorages,
				instanceCarsResult,
				report,
				withPrices);
			
			CreateBulkRows(
				cancellationToken,
				ref counter,
				noms,
				reservedItems,
				prices,
				alternativePrices,
				purchasePrices,
				minStockResult,
				warehouseStorages,
				bulkWarehousesResult,
				employeeStorages,
				bulkEmployeesResult,
				carStorages,
				bulkCarsResult,
				report);

			RemoveStoragesByFilterCondition(report, cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();
			return await new ValueTask<BalanceSummaryReport>(report);
		}

		private void CreateInstanceRows(
			CancellationToken cancellationToken,
			ref int counter,
			List<SelectableParameter> instances,
			InstanceData[] instanceData,
			List<SelectableParameter> warehouseStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceWarehousesResult,
			List<SelectableParameter> employeeStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> instanceEmployeesResult,
			List<SelectableParameter> carStorages,
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
					InventoryNumber = instanceData[instancesCounter].InventoryNumber,
					WarehousesBalances = new List<decimal>(),
					EmployeesBalances = new List<decimal>(),
					CarsBalances = new List<decimal>()
				};

				if(withPrices)
				{
					row.PurchasePrice = instanceData[instancesCounter].PurchasePrice;
				}

				FillStoragesBalance(row.WarehousesBalances, warehouseStorages, instanceId, instanceWarehousesResult, cancellationToken);
				FillStoragesBalance(row.EmployeesBalances, employeeStorages, instanceId, instanceEmployeesResult, cancellationToken);
				FillStoragesBalance(row.CarsBalances, carStorages, instanceId, instanceCarsResult, cancellationToken);

				AddRow(report, row);
			}
		}

		private void CreateBulkRows(
			CancellationToken cancellationToken,
			ref int counter,
			List<SelectableParameter> noms,
			List<ReservedBalance> reservedItems,
			List<PriceNode> prices,
			List<PriceNode> alternativePrices,
			List<PriceNode> purchasePrices,
			decimal[] minStockResult,
			List<SelectableParameter> warehouseStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkWarehousesResult,
			List<SelectableParameter> employeeStorages,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> bulkEmployeesResult,
			List<SelectableParameter> carStorages,
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
					Min = minStockResult[nomsCounter]
				};

				FillStoragesBalance(row.WarehousesBalances, warehouseStorages, nomenclatureId, bulkWarehousesResult, cancellationToken);
				FillStoragesBalance(row.EmployeesBalances, employeeStorages, nomenclatureId, bulkEmployeesResult, cancellationToken);
				FillStoragesBalance(row.CarsBalances, carStorages, nomenclatureId, bulkCarsResult, cancellationToken);

				AddRow(report, row);
			}
		}

		private void FillStoragesBalance(
			IList<decimal> storagesBalances,
			IList<SelectableParameter> storages,
			int entityId,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> storagesResult,
			CancellationToken cancellationToken)
		{
			for(var i = 0; i < storages?.Count; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				var key = new NomenclatureStorageIds(entityId, (int)storages[i].Value);
				storagesResult.TryGetValue(key, out var tempBulkBalanceBean);
				var amount = tempBulkBalanceBean?.Amount ?? 0;
				
				storagesBalances.Add(amount);
			}
		}

		public void ExportReport()
		{
			using(var wb = new XLWorkbook())
			{
				var sheetName = $"{DateTime.Now:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				InsertValues(ws, _isCreatedWithReserveData);
				ws.Columns().AdjustToContents();

				if(TryGetSavePath(out string path))
				{
					wb.SaveAs(path);
				}
			}
		}

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

		private void InsertValues(IXLWorksheet ws, bool withReserveData)
		{
			var colNames = GetColumnTitles(withReserveData);
			var rows = GetRowsData(withReserveData);
			
			var index = 1;
			foreach(var name in colNames)
			{
				ws.Cell(1, index).Value = name;
				index++;
			}

			ws.Cell(2, 1).InsertData(rows);
			AddStoragesColumns(ws, index);
		}

		private static string[] GetColumnTitles(bool withReserveData)
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
		
		private IEnumerable GetRowsData(bool withReserveData)
		{
			if(!withReserveData)
			{
				return from row in Report.SummaryRows
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
			
			return from row in Report.SummaryRows
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
			for(var i = 0; i < Report.WarehouseStoragesTitles?.Count; i++)
			{
				ws.Cell(1, startIndex).Value = $"{Report.WarehouseStoragesTitles[i]}";
				ws.Cell(2, startIndex).InsertData(Report.SummaryRows.Select(sr => sr.WarehousesBalances[i]));
				startIndex++;
			}
		}
		
		private void AddEmployeeColumns(IXLWorksheet ws, ref int startIndex)
		{
			for(var i = 0; i < Report.EmployeeStoragesTitles?.Count; i++)
			{
				ws.Cell(1, startIndex).Value = $"{Report.EmployeeStoragesTitles[i]}";
				ws.Cell(2, startIndex).InsertData(Report.SummaryRows.Select(sr => sr.EmployeesBalances[i]));
				startIndex++;
			}
		}
		
		private void AddCarColumns(IXLWorksheet ws, ref int startIndex)
		{
			for(var i = 0; i < Report.CarStoragesTitles?.Count; i++)
			{
				ws.Cell(1, startIndex).Value = $"{Report.CarStoragesTitles[i]}";
				ws.Cell(2, startIndex).InsertData(Report.SummaryRows.Select(sr => sr.CarsBalances[i]));
				startIndex++;
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

		private void AddRow(BalanceSummaryReport report, BalanceSummaryRow row)
		{
			if(AllNomenclatures
				|| IsGreaterThanZeroByNomenclature && row.WarehousesBalances.FirstOrDefault(war => war > 0) > 0
				|| IsLessOrEqualZeroByNomenclature && row.WarehousesBalances.FirstOrDefault(war => war <= 0) <= 0
				|| IsLessThanMinByNomenclature && row.WarehousesBalances.FirstOrDefault(war => war < row.Min) < row.Min
				|| IsGreaterOrEqualThanMinByNomenclature && row.WarehousesBalances.FirstOrDefault(war => war >= row.Min) >= row.Min)
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
						var query = UoW.Session.QueryOver<Employee>()
							.Where(e => e.Status == EmployeeStatus.IsWorking);

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
							.Select(employeeName).WithAlias(() => resultAlias.EntityTitle)
						).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
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

						var query = UoW.Session.QueryOver(() => carAlias)
							.JoinAlias(c => c.CarModel, () => carModelAlias)
							.Where(x => !x.IsArchive);

						var customName = CustomProjections.Concat(
							Projections.Property(() => carModelAlias.Name),
							Projections.Constant(" ("),
							Projections.Property(() => carAlias.RegistrationNumber),
							Projections.Constant(")"));

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
							.Select(customName).WithAlias(() => resultAlias.EntityTitle)
						).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Car>>());
						return query.List<SelectableParameter>();
					})));

			return new SelectableParameterReportFilterViewModel(_storagesFilter);
		}

		#endregion
	}
}


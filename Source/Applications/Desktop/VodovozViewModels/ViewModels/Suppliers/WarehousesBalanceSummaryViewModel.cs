using System;
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
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	//TODO проверить работу вью модели
	public class WarehousesBalanceSummaryViewModel : DialogTabViewModelBase
	{
		private const string _xlsxFileFilter = "XLSX File (*.xlsx)";
		private const string _parameterNom = nameof(Nomenclature);
		private const string _parameterNomType = nameof(NomenclatureCategory);
		private const string _parameterProductGroups = nameof(ProductGroup);
		private const string _parameterWarehouseStorages = nameof(Warehouse);
		private const string _parameterEmployeeStorages = nameof(Employee);
		private const string _parameterCarStorages = nameof(Car);
		private const string _bulkBalanceWarehousesKey = "bulkBalanceWarehouses";
		private const string _bulkBalanceEmployeesKey = "bulkBalanceEmployees";
		private const string _bulkBalanceCarsKey = "bulkBalanceCars";
		private const string _instanceBalanceWarehousesKey = "instanceBalanceWarehouses";
		private const string _instanceBalanceEmployeesKey = "instanceBalanceEmployees";
		private const string _instanceBalanceCarsKey = "instanceBalanceCars";
		private const string _minStockKey = "minStock";
		private readonly IFileDialogService _fileDialogService;
		private SelectableParameterReportFilterViewModel _nomsViewModel;
		private SelectableParameterReportFilterViewModel _storagesViewModel;
		private SelectableParametersReportFilter _nomsFilter;
		private SelectableParametersReportFilter _storagesFilter;

		private bool _isGenerating = false;
		private BalanceSummaryReport _report;

		public WarehousesBalanceSummaryViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation, IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_fileDialogService = fileDialogService;
			TabName = "Остатки";
		}

		#region Свойства

		public DateTime? EndDate { get; set; } = DateTime.Today;

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
				return await GenerateAsync(EndDate ?? DateTime.Today, uow, cancellationToken);
			}
			finally
			{
				uow.Dispose();
			}
		}

		private async Task<BalanceSummaryReport> GenerateAsync(
			DateTime endDate,
			IUnitOfWork localUow,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);

			var nomsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNom);
			var noms = nomsSet?.GetIncludedParameters()?.ToList();
			var typesSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNomType);
			var types = typesSet?.GetIncludedParameters()?.ToList();
			var groupsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterProductGroups);
			var groups = groupsSet?.GetIncludedParameters()?.ToList();
			var warehouseStorages = _storagesFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == _parameterWarehouseStorages)?.GetIncludedParameters()?.ToList();
			var employeeStorages = _storagesFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == _parameterEmployeeStorages)?.GetIncludedParameters()?.ToList();
			var carStorages = _storagesFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == _parameterCarStorages)?.GetIncludedParameters()?.ToList();

			Nomenclature nomAlias = null;

			var warehousesIds = warehouseStorages?.Select(x => (int)x.Value).ToArray();
			var employeesIds = employeeStorages?.Select(x => (int)x.Value).ToArray();
			var carsIds = carStorages?.Select(x => (int)x.Value).ToArray();
			var groupsIds = groups?.Select(x => (int)x.Value).ToArray();
			var groupsSelected = groups?.Any() ?? false;
			var typesSelected = types?.Any() ?? false;
			var nomsSelected = noms?.Any() ?? false;
			var allNomsSelected = noms?.Count == nomsSet?.Parameters.Count;
			if(groupsSelected)
			{
				var nomsInGroupsIds = (List<int>)await localUow.Session.QueryOver(() => nomAlias)
					.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds))
					.AndNot(() => nomAlias.IsArchive)
					.Select(n => n.Id).ListAsync<int>(cancellationToken);
				if(nomsSelected)
				{
					noms = noms.Where(x => nomsInGroupsIds.Contains((int)x.Value)).ToList();
				}
				else
				{
					noms?.AddRange(nomsSet.Parameters.Where(x => nomsInGroupsIds.Contains((int)x.Value)).ToList());
				}
			}

			if(!nomsSelected && !groupsSelected)
			{
				noms?.AddRange(nomsSet.Parameters);
			}

			var nomsIds = noms?.Select(x => (int)x.Value).ToArray();

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
			InventoryNomenclatureInstance inventoryNomenclatureInstance = null;
			BalanceBean resultAlias = null;

			IQueryOver<WarehouseBulkGoodsAccountingOperation, WarehouseBulkGoodsAccountingOperation> bulkBalanceByWarehousesQuery = null;
			IQueryOver<WarehouseInstanceGoodsAccountingOperation, WarehouseInstanceGoodsAccountingOperation> instanceBalanceByWarehousesQuery = null;

			if(warehousesIds != null)
			{
				bulkBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseBulkOperationAlias)
					.Where(() => warehouseBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseBulkOperationAlias.Warehouse.Id).IsIn(warehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseBulkOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(Projections.Constant("-")).WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => warehouseBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => warehouseBulkOperationAlias.Warehouse.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			
				instanceBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseInstanceOperationAlias)
					.JoinAlias(() => warehouseInstanceOperationAlias.InventoryNomenclatureInstance, () => inventoryNomenclatureInstance)
					.Where(() => warehouseInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseInstanceOperationAlias.Warehouse.Id).IsIn(warehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseInstanceOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => inventoryNomenclatureInstance.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => warehouseInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => warehouseInstanceOperationAlias.Warehouse.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			IQueryOver<EmployeeBulkGoodsAccountingOperation, EmployeeBulkGoodsAccountingOperation> bulkBalanceByEmployeesQuery = null;
			IQueryOver<EmployeeInstanceGoodsAccountingOperation, EmployeeInstanceGoodsAccountingOperation> instanceBalanceByEmployeesQuery = null;

			if(employeesIds != null)
			{
				bulkBalanceByEmployeesQuery = localUow.Session.QueryOver(() => employeeBulkOperationAlias)
					.Where(() => employeeBulkOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeBulkOperationAlias.Employee.Id).IsIn(employeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeBulkOperationAlias.Employee.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(Projections.Constant("-")).WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => employeeBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => employeeBulkOperationAlias.Employee.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			
				instanceBalanceByEmployeesQuery = localUow.Session.QueryOver(() => employeeInstanceOperationAlias)
					.JoinAlias(() => employeeInstanceOperationAlias.InventoryNomenclatureInstance, () => inventoryNomenclatureInstance)
					.Where(() => employeeInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeInstanceOperationAlias.Employee.Id).IsIn(employeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeInstanceOperationAlias.Employee.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => inventoryNomenclatureInstance.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => employeeInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
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
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(Projections.Constant("-")).WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => carBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => carBulkOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			
				instanceBalanceByCarsQuery = localUow.Session.QueryOver(() => carInstanceOperationAlias)
					.JoinAlias(() => carInstanceOperationAlias.InventoryNomenclatureInstance, () => inventoryNomenclatureInstance)
					.Where(() => carInstanceOperationAlias.OperationTime <= endDate)
					.And(() => !nomAlias.IsArchive)
					.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => carInstanceOperationAlias.Car.Id).IsIn(carsIds)
					.SelectList(list => list
						.SelectGroup(() => carInstanceOperationAlias.Car.Id).WithAlias(() => resultAlias.StorageId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => inventoryNomenclatureInstance.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
						.SelectSum(() => carInstanceOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => carInstanceOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			var minStockQuery = localUow.Session.QueryOver(() => nomAlias)
			.Where(() => !nomAlias.IsArchive)
			.Select(n => n.MinStockCount)
			.OrderBy(n => n.Id).Asc;

			if(typesSelected)
			{
				var typesIds = types.Select(x => (int)x.Value).ToArray();

				bulkBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				instanceBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				bulkBalanceByEmployeesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				instanceBalanceByEmployeesQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				bulkBalanceByCarsQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				instanceBalanceByCarsQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				minStockQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
			}

			if(nomsSelected && !allNomsSelected)
			{
				bulkBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				instanceBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				bulkBalanceByEmployeesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				instanceBalanceByEmployeesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				bulkBalanceByCarsQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				instanceBalanceByCarsQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				minStockQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
			}

			if(groupsSelected)
			{
				bulkBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				instanceBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				bulkBalanceByEmployeesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				instanceBalanceByEmployeesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				bulkBalanceByCarsQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				instanceBalanceByCarsQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				minStockQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
			}

			#endregion

			ILookup<int, BalanceBean> bulkWarehousesResult = null;
			ILookup<int, BalanceBean> instanceWarehousesResult = null;
			ILookup<int, BalanceBean> bulkEmployeesResult = null;
			ILookup<int, BalanceBean> instanceEmployeesResult = null;
			ILookup<int, BalanceBean> bulkCarsResult = null;
			ILookup<int, BalanceBean> instanceCarsResult = null;
					
			var batch = localUow.Session.CreateQueryBatch()
				.Add<decimal>(_minStockKey, minStockQuery);

			#region fillbatchQuery

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
						.ToLookup(x => x.NomId);
				//.GroupBy(x => x.NomId)
				//.ToDictionary(x => x.Key);
			}

			if(instanceBalanceByWarehousesQuery != null)
			{
				instanceWarehousesResult =
					batch.GetResult<BalanceBean>(_instanceBalanceWarehousesKey)
						.ToLookup(x => x.NomId);
			}

			if(bulkBalanceByEmployeesQuery != null)
			{
				bulkEmployeesResult =
					batch.GetResult<BalanceBean>(_bulkBalanceEmployeesKey)
						.ToLookup(x => x.NomId);
			}

			if(instanceBalanceByEmployeesQuery != null)
			{
				instanceEmployeesResult =
					batch.GetResult<BalanceBean>(_instanceBalanceEmployeesKey)
						.ToLookup(x => x.NomId);
			}

			if(bulkBalanceByCarsQuery != null)
			{
				bulkCarsResult =
					batch.GetResult<BalanceBean>(_bulkBalanceCarsKey)
						.ToLookup(x => x.NomId);
			}

			if(instanceBalanceByCarsQuery != null)
			{
				instanceCarsResult =
					batch.GetResult<BalanceBean>(_instanceBalanceCarsKey)
						.ToLookup(x => x.NomId);
			}
			
			var minStockResult = batch.GetResult<decimal>(_minStockKey).ToArray();

			#endregion

			var id = 0;
			BalanceSummaryRow row = null;
			
			/*foreach(var item in bulkWarehouseResult)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				if(id != item.NomId)
				{
					row = new BalanceSummaryRow
					{
						NomId = item.NomId,
						NomTitle = item.NomTitle,
						InventoryNumber = item.InventoryNumber,
						WarehousesBalances = new List<decimal>(),
						Min = item.MinStockCount
					};

					id = item.NomId;
					AddRow(ref report, row);
				}
				row.WarehousesBalances.Add(item.Amount);
			}*/
			
			/*for(int i = 0; i < bulkWarehouseResult.Count; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var row = new BalanceSummaryRow
				{
					NomId = (int)noms[i].Value,
					NomTitle = noms[i].Title,
					InventoryNumber = bulkWarehouseResult[i].InventoryNumber,
					Separate = new List<decimal>(),
					Min = 0m //bulkWarehouseResult[]
				};
				
				AddRow(ref report, row);
			}*/

			for(var nomsCounter = 0; nomsCounter < noms?.Count; nomsCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var nomenclatureId = (int)noms[nomsCounter].Value;
				row = new BalanceSummaryRow
				{
					NomId = nomenclatureId,
					NomTitle = noms[nomsCounter].Title,
					WarehousesBalances = new List<decimal>(),
					EmployeesBalances = new List<decimal>(),
					CarsBalances = new List<decimal>(),
					Min = minStockResult[nomsCounter]
				};

				for(var warsCounter = 0; warsCounter < warehouseStorages?.Count; warsCounter++)
				{
					row.WarehousesBalances.Add(0);
					//Т.к. данные запросов упорядочены, тут реализован доступ по индексам
					var warId = (int)warehouseStorages[warsCounter].Value;

					if(bulkWarehousesResult.Contains(nomenclatureId))
					{
						var tempBulkBalanceBean = bulkWarehousesResult[nomenclatureId].ElementAtOrDefault(warsCounter);

						if(tempBulkBalanceBean != null && tempBulkBalanceBean.StorageId == warId)
						{
							row.WarehousesBalances[warsCounter] += tempBulkBalanceBean.Amount;
						}
					}

					if(!instanceWarehousesResult.Contains(nomenclatureId))
					{
						continue;
					}

					var tempInstanceBalanceBean = bulkWarehousesResult[nomenclatureId].ElementAtOrDefault(warsCounter);

					if(tempInstanceBalanceBean != null && tempInstanceBalanceBean.StorageId == warId)
					{
						row.WarehousesBalances[warsCounter] += tempInstanceBalanceBean.Amount;
					}
				}
				
				for(var employeesCounter = 0; employeesCounter < employeeStorages?.Count; employeesCounter++)
				{
					row.EmployeesBalances.Add(0);
					//Т.к. данные запросов упорядочены, тут реализован доступ по индексам
					var warId = (int)employeeStorages[employeesCounter].Value;

					if(bulkWarehousesResult.Contains(nomenclatureId))
					{
						var tempBulkBalanceBean = bulkEmployeesResult[nomenclatureId].ElementAtOrDefault(employeesCounter);

						if(tempBulkBalanceBean != null && tempBulkBalanceBean.StorageId == warId)
						{
							row.EmployeesBalances[employeesCounter] += tempBulkBalanceBean.Amount;
						}
					}

					if(!instanceEmployeesResult.Contains(nomenclatureId))
					{
						continue;
					}

					var tempInstanceBalanceBean = instanceEmployeesResult[nomenclatureId].ElementAtOrDefault(employeesCounter);

					if(tempInstanceBalanceBean != null && tempInstanceBalanceBean.StorageId == warId)
					{
						row.EmployeesBalances[employeesCounter] += tempInstanceBalanceBean.Amount;
					}
				}
				
				for(var carsCounter = 0; carsCounter < carStorages?.Count; carsCounter++)
				{
					row.CarsBalances.Add(0);
					//Т.к. данные запросов упорядочены, тут реализован доступ по индексам
					var warId = (int)carStorages[carsCounter].Value;

					if(bulkCarsResult.Contains(nomenclatureId))
					{
						var tempBulkBalanceBean = bulkCarsResult[nomenclatureId].ElementAtOrDefault(carsCounter);

						if(tempBulkBalanceBean != null && tempBulkBalanceBean.StorageId == warId)
						{
							row.CarsBalances[carsCounter] += tempBulkBalanceBean.Amount;
						}
					}

					if(!instanceCarsResult.Contains(nomenclatureId))
					{
						continue;
					}

					var tempInstanceBalanceBean = instanceCarsResult[nomenclatureId].ElementAtOrDefault(carsCounter);

					if(tempInstanceBalanceBean != null && tempInstanceBalanceBean.StorageId == warId)
					{
						row.CarsBalances[carsCounter] += tempInstanceBalanceBean.Amount;
					}
				}

				AddRow(ref report, row);
			}

			RemoveWarehousesByFilterCondition(ref report, cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();
			return await new ValueTask<BalanceSummaryReport>(report);
		}

		public void ExportReport()
		{
			using(var wb = new XLWorkbook())
			{
				var sheetName = $"{DateTime.Now:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				InsertValues(ws);
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

		private void InsertValues(IXLWorksheet ws)
		{
			var colNames = new string[] { "Код", "Наименование", "Мин. Остаток", "Общий остаток", "Разница" };
			var rows = from row in Report.SummaryRows
					   select new
					   {
						   row.NomId,
						   row.NomTitle,
						   row.Min,
						   row.Common,
						   row.Diff
					   };
			int index = 1;
			foreach(var name in colNames)
			{
				ws.Cell(1, index).Value = name;
				index++;
			}
			ws.Cell(2, 1).InsertData(rows);
			AddWarehouseColumns(ws, index);
		}

		private void AddWarehouseColumns(IXLWorksheet ws, int startIndex)
		{
			for(var i = 0; i < Report.WarehouseStoragesTitles.Count; i++)
			{
				ws.Cell(1, startIndex + i).Value = $"{Report.WarehouseStoragesTitles[i]}";
				ws.Cell(2, startIndex + i).InsertData(Report.SummaryRows.Select(sr => sr.WarehousesBalances[i]));
			}
		}

		private void RemoveWarehousesByFilterCondition(ref BalanceSummaryReport report, CancellationToken cancellationToken)
		{
			if(AllWarehouses)
			{
				return;
			}

			for(var warCounter = 0; warCounter < report.WarehouseStoragesTitles.Count; warCounter++)
			{
				if(IsGreaterThanZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.WarehousesBalances[warCounter] > 0) == null
				|| IsLessOrEqualZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.WarehousesBalances[warCounter] <= 0) == null
				|| IsLessThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min < row.WarehousesBalances[warCounter]) == null
				|| IsGreaterOrEqualThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min >= row.WarehousesBalances[warCounter]) == null)
				{
					RemoveWarehouseByIndex(ref report, ref warCounter, cancellationToken);
				}
			}
		}

		private void RemoveWarehouseByIndex(ref BalanceSummaryReport report, ref int warCounter, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			report.WarehouseStoragesTitles.RemoveAt(warCounter);
			var i = warCounter;
			report.SummaryRows.ForEach(row => row.WarehousesBalances.RemoveAt(i));
			warCounter--;
		}

		private void AddRow(ref BalanceSummaryReport report, BalanceSummaryRow row)
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

		private void AddRows(ref BalanceSummaryReport report, IEnumerable<BalanceSummaryRow> rows)
		{
			foreach(var row in rows)
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
		}

		#region Настройка фильтров

		private SelectableParameterReportFilterViewModel CreateNomsViewModel()
		{
			_nomsFilter = new SelectableParametersReportFilter(UoW);
			var nomenclatureTypeParam = _nomsFilter.CreateParameterSet("Типы номенклатур", _parameterNomType,
				new ParametersEnumFactory<NomenclatureCategory>());

			var nomenclatureParam = _nomsFilter.CreateParameterSet("Номенклатуры", _parameterNom,
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = UoW.Session.QueryOver<Nomenclature>().Where(x => !x.IsArchive);
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
					).TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Nomenclature>>());
					return query.List<SelectableParameter>();
				})
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues();
					return !EnumerableExtensions.Any(selectedValues)
						? null
						: nomenclatureTypeParam.FilterType == SelectableFilterType.Include
							? Restrictions.On<Nomenclature>(x => x.Category).IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray())
							: Restrictions.On<Nomenclature>(x => x.Category).Not.IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				}
			);

			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			_nomsFilter.CreateParameterSet("Группы товаров", _parameterProductGroups,
				new RecursiveParametersFactory<ProductGroup>(UoW, (filters) =>
					{
						var query = UoW.Session.QueryOver<ProductGroup>()
							.Where(p => p.Parent == null);
						
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

			return new SelectableParameterReportFilterViewModel(_nomsFilter);
		}

		private SelectableParameterReportFilterViewModel CreateStoragesViewModel()
		{
			_storagesFilter = new SelectableParametersReportFilter(UoW);

			StoragesParametersSets.Add(_storagesFilter.CreateParameterSet(
				"Склады",
				_parameterWarehouseStorages,
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
				_parameterEmployeeStorages,
				new ParametersFactory(
					UoW,
					filters =>
					{
						SelectableEntityParameter<Employee> resultAlias = null;
						var query = UoW.Session.QueryOver<Employee>()
							.Where(e => e.Status == EmployeeStatus.IsWorking);
						
						var employeeName = CustomProjections.Concat_WS(
							" ",
							Projections.Property<Employee>(x => x.LastName),
							Projections.Property<Employee>(x => x.Name),
							Projections.Property<Employee>(x => x.Patronymic)
						);
						
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
				_parameterCarStorages,
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

	public class SummaryBalanceHandlerFactory
	{
		public SummaryBalanceHandler CreateNewSummaryBalanceHandler(
			ILookup<int, BalanceBean> bulkWarehousesResult,
			ILookup<int, BalanceBean> instanceWarehousesResult
			)
		{
			var bulkWarehousesBalanceHandler = new BalanceNodeHandler(bulkWarehousesResult);
			var instanceWarehousesBalanceHandler = new BalanceNodeHandler(instanceWarehousesResult);
			
			bulkWarehousesBalanceHandler.SetNextHandler(instanceWarehousesBalanceHandler);
			
			//var bulkEmployeesBalanceHandler = new BalanceNodeHandler();
			//var instanceEmployeesBalanceHandler = new BalanceNodeHandler();
			
			//bulkEmployeesBalanceHandler.SetNextHandler(instanceEmployeesBalanceHandler);
			
			//var bulkCarsBalanceHandler = new BalanceNodeHandler();
			//var instanceCarsBalanceHandler = new BalanceNodeHandler();
			
			//bulkCarsBalanceHandler.SetNextHandler(instanceCarsBalanceHandler);

			return new SummaryBalanceHandler(bulkWarehousesBalanceHandler);
		}
	}

	public class SummaryBalanceHandler
	{
		private readonly BalanceNodeHandler.IBalanceNodeHandler _warehousesBalanceHandler;
		private readonly BalanceNodeHandler.IBalanceNodeHandler _employeesBalanceHandler;
		private readonly BalanceNodeHandler.IBalanceNodeHandler _carsBalanceHandler;

		public SummaryBalanceHandler(
			BalanceNodeHandler.IBalanceNodeHandler warehousesBalanceHandler)
			//BalanceNodeHandler.IBalanceNodeHandler employeesBalanceHandler,
			//BalanceNodeHandler.IBalanceNodeHandler carsBalanceHandler)
		{
			_warehousesBalanceHandler = warehousesBalanceHandler ?? throw new ArgumentNullException(nameof(warehousesBalanceHandler));
			//_employeesBalanceHandler = employeesBalanceHandler ?? throw new ArgumentNullException(nameof(employeesBalanceHandler));
			//_carsBalanceHandler = carsBalanceHandler ?? throw new ArgumentNullException(nameof(carsBalanceHandler));
		}
		
		public IList<BalanceSummaryRow> HandleBalances(
			IList<SelectableParameter> noms,
			IList<SelectableParameter> warehouseStorages,
			IList<SelectableParameter> employeeStorages,
			IList<SelectableParameter> carStorages,
			decimal[] minStockResult,
			CancellationToken cancellationToken)
		{
			var rows = new List<BalanceSummaryRow>();
			
			for(var nomsCounter = 0; nomsCounter < noms?.Count; nomsCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var nomenclatureId = (int)noms[nomsCounter].Value;
				
				var row = new BalanceSummaryRow
				{
					NomId = nomenclatureId,
					NomTitle = noms[nomsCounter].Title,
					WarehousesBalances = new List<decimal>(),
					EmployeesBalances = new List<decimal>(),
					CarsBalances = new List<decimal>(),
					Min = minStockResult[nomsCounter]
				};

				HandleBalance(_warehousesBalanceHandler, warehouseStorages, row.WarehousesBalances, nomenclatureId, cancellationToken);
				HandleBalance(_employeesBalanceHandler, employeeStorages, row.EmployeesBalances, nomenclatureId, cancellationToken);
				HandleBalance(_carsBalanceHandler, carStorages, row.CarsBalances, nomenclatureId, cancellationToken);
				
				rows.Add(row);
			}

			return rows;
		}

		private void HandleBalance(
			BalanceNodeHandler.IBalanceNodeHandler balanceNodeHandler,
			IList<SelectableParameter> storages,
			IList<decimal> balances,
			int nomenclatureId,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			
			for(var index = 0; index < storages?.Count; index++)
			{
				balances.Add(0);
				var storageId = (int)storages[index].Value;

				balanceNodeHandler.HandleBalance(balances, storageId, nomenclatureId, index, cancellationToken);
			}
		}
	}

	public class BalanceNodeHandler : BalanceNodeHandler.IBalanceNodeHandler
	{
		private readonly ILookup<int, BalanceBean> _balanceResult;
		private IBalanceNodeHandler _nextHandler;

		public BalanceNodeHandler(ILookup<int, BalanceBean> balanceResult)
		{
			_balanceResult = balanceResult ?? throw new ArgumentNullException(nameof(balanceResult));
		}
		
		public void SetNextHandler(IBalanceNodeHandler nextHandler)
		{
			_nextHandler = nextHandler;
		}

		public void HandleBalance(IList<decimal> balances, int storageId, int nomenclatureId, int index, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			
			if(_balanceResult.Contains(nomenclatureId))
			{
				var tempBalanceBean = _balanceResult[nomenclatureId].ElementAtOrDefault(index);

				if(tempBalanceBean != null && tempBalanceBean.StorageId == storageId)
				{
					balances[index] += tempBalanceBean.Amount;
				}
			}

			_nextHandler?.HandleBalance(balances, storageId, nomenclatureId, index, cancellationToken);
		}

		public interface IBalanceNodeHandler
		{
			void SetNextHandler(IBalanceNodeHandler nextHandler);
			void HandleBalance(IList<decimal> balances, int storageId, int nomenclatureId, int index, CancellationToken cancellationToken);
		}
	}

	public class BalanceSummaryRow
	{
		public int NomId { get; set; }
		public string NomTitle { get; set; }
		public string InventoryNumber { get; set; }
		public decimal Min { get; set; }
		public decimal Common => WarehousesBalances.Sum() + EmployeesBalances.Sum() + CarsBalances.Sum();
		public decimal Diff => Common - Min;
		public List<decimal> WarehousesBalances { get; set; }
		public List<decimal> EmployeesBalances { get; set; }
		public List<decimal> CarsBalances { get; set; }
	}

	public class BalanceSummaryReport
	{
		public DateTime EndDate { get; set; }
		public List<string> WarehouseStoragesTitles { get; set; }
		public List<string> EmployeeStoragesTitles { get; set; }
		public List<string> CarStoragesTitles { get; set; }
		public List<BalanceSummaryRow> SummaryRows { get; set; }
	}

	public class BalanceBean
	{
		public int NomId { get; set; }
		public string NomTitle { get; set; }
		public int? StorageId { get; set; }
		public string InventoryNumber { get; set; }
		public decimal Amount { get; set; }
		public decimal MinStockCount { get; set; }
	}
}

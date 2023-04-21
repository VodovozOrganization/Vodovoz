using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
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
			TabName = "Остатки по складам";
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
			GoodsAccountingOperation woAlias = null;
			BalanceBean resultAlias = null;

			IQueryOver<WarehouseBulkGoodsAccountingOperation, WarehouseBulkGoodsAccountingOperation> bulkBalanceByWarehousesQuery = null;
			IQueryOver<WarehouseInstanceGoodsAccountingOperation, WarehouseInstanceGoodsAccountingOperation> instanceBalanceByWarehousesQuery = null;

			if(warehousesIds != null)
			{
				bulkBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseBulkOperationAlias)
					.Where(() => warehouseBulkOperationAlias.OperationTime <= endDate)
					.AndNot(() => nomAlias.IsArchive)
					.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseBulkOperationAlias.Warehouse.Id).IsIn(warehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseBulkOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.WarehouseId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => nomAlias.MinStockCount).WithAlias(() => resultAlias.MinStockCount)
						.SelectSum(() => warehouseBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => warehouseBulkOperationAlias.Warehouse.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
				
				instanceBalanceByWarehousesQuery = localUow.Session.QueryOver(() => warehouseInstanceOperationAlias)
					.Where(() => warehouseInstanceOperationAlias.OperationTime <= endDate)
					.AndNot(() => nomAlias.IsArchive)
					.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => warehouseInstanceOperationAlias.Warehouse.Id).IsIn(warehousesIds)
					.SelectList(list => list
						.SelectGroup(() => warehouseInstanceOperationAlias.Warehouse.Id).WithAlias(() => resultAlias.WarehouseId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => nomAlias.MinStockCount).WithAlias(() => resultAlias.MinStockCount)
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
					.AndNot(() => nomAlias.IsArchive)
					.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeBulkOperationAlias.Employee.Id).IsIn(employeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeBulkOperationAlias.Employee.Id).WithAlias(() => resultAlias.WarehouseId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => nomAlias.MinStockCount).WithAlias(() => resultAlias.MinStockCount)
						.SelectSum(() => employeeBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => employeeBulkOperationAlias.Employee.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
				
				instanceBalanceByEmployeesQuery = localUow.Session.QueryOver(() => employeeInstanceOperationAlias)
					.Where(() => employeeBulkOperationAlias.OperationTime <= endDate)
					.AndNot(() => nomAlias.IsArchive)
					.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => employeeBulkOperationAlias.Employee.Id).IsIn(employeesIds)
					.SelectList(list => list
						.SelectGroup(() => employeeBulkOperationAlias.Employee.Id).WithAlias(() => resultAlias.WarehouseId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => nomAlias.MinStockCount).WithAlias(() => resultAlias.MinStockCount)
						.SelectSum(() => employeeBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => employeeBulkOperationAlias.Employee.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			IQueryOver<CarBulkGoodsAccountingOperation, CarBulkGoodsAccountingOperation> bulkBalanceByCarsQuery = null;
			IQueryOver<CarInstanceGoodsAccountingOperation, CarInstanceGoodsAccountingOperation> instanceBalanceByCarsQuery = null;

			if(carsIds != null)
			{
				bulkBalanceByCarsQuery = localUow.Session.QueryOver(() => carBulkOperationAlias)
					.Where(() => carBulkOperationAlias.OperationTime <= endDate)
					.AndNot(() => nomAlias.IsArchive)
					.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => carBulkOperationAlias.Car.Id).IsIn(carsIds)
					.SelectList(list => list
						.SelectGroup(() => carBulkOperationAlias.Car.Id).WithAlias(() => resultAlias.WarehouseId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => nomAlias.MinStockCount).WithAlias(() => resultAlias.MinStockCount)
						.SelectSum(() => carBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => carBulkOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
				
				instanceBalanceByCarsQuery = localUow.Session.QueryOver(() => carInstanceOperationAlias)
					.Where(() => carBulkOperationAlias.OperationTime <= endDate)
					.AndNot(() => nomAlias.IsArchive)
					.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
					.WhereRestrictionOn(() => carBulkOperationAlias.Car.Id).IsIn(carsIds)
					.SelectList(list => list
						.SelectGroup(() => carBulkOperationAlias.Car.Id).WithAlias(() => resultAlias.WarehouseId)
						.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
						.Select(() => nomAlias.MinStockCount).WithAlias(() => resultAlias.MinStockCount)
						.SelectSum(() => carBulkOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					)
					.OrderBy(() => nomAlias.Id).Asc
					.ThenBy(() => carBulkOperationAlias.Car.Id).Asc
					.TransformUsing(Transformers.AliasToBean<BalanceBean>());
			}

			/*var minStockQuery = localUow.Session.QueryOver(() => nomAlias)
				.WhereNot(() => nomAlias.IsArchive)
				.Select(n => n.MinStockCount)
				.OrderBy(n => n.Id).Asc;*/

			if(typesSelected)
			{
				var typesIds = types.Select(x => (int)x.Value).ToArray();

				bulkBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				bulkBalanceByEmployeesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				bulkBalanceByCarsQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				//woQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				//minStockQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
			}

			if(nomsSelected && !allNomsSelected)
			{
				bulkBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				bulkBalanceByEmployeesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				bulkBalanceByCarsQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				//woQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				//minStockQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
			}

			if(groupsSelected)
			{
				bulkBalanceByWarehousesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				bulkBalanceByEmployeesQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				bulkBalanceByCarsQuery?.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				//woQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				//minStockQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
			}

			#endregion

			var batch = localUow.Session.CreateQueryBatch()
				.Add<BalanceBean>("in", bulkBalanceByWarehousesQuery);
				//.Add<BalanceBean>("wo", woQuery)
				//.Add<decimal>("ms", minStockQuery);

			var bulkWarehouseResult =
				batch.GetResult<BalanceBean>("bulkWarehouse").ToDictionary(x => x.NomId);
			var instanceWarehouseResult =
				batch.GetResult<BalanceBean>("instanceWarehouse").ToDictionary(x => x.NomId);
			//var woResult = batch.GetResult<BalanceBean>("wo").ToArray();
			//var msResult = batch.GetResult<decimal>("ms").ToArray();

			for(int i = 0; i < bulkWarehouseResult.Count; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var row = new BalanceSummaryRow
				{
					NomId = (int)noms[i].Value,
					NomTitle = noms[i].Title,
					Separate = new List<decimal>(),
					Min = 0m //bulkWarehouseResult[]
				};
				
				AddRow(ref report, row);
			}
			
			//Кол-во списаний != кол-во начислений, используется два счетчика
			/*var addedCounter = 0;
			var removedCounter = 0;
			for(var nomsCounter = 0; nomsCounter < noms?.Count; nomsCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var row = new BalanceSummaryRow
				{
					NomId = (int)noms[nomsCounter].Value,
					NomTitle = noms[nomsCounter].Title,
					Separate = new List<decimal>(),
					Min = noms[nomsCounter].
				};

				for(var warsCounter = 0; warsCounter < warehouseStorages?.Count; warsCounter++)
				{
					row.Separate.Add(0);
					//Т.к. данные запросов упорядочены, тут реализован доступ по индексам
					var warId = (int)warehouseStorages[warsCounter].Value;
					if(addedCounter != inResult.Length)
					{
						var tempIn = inResult[addedCounter];
						if(tempIn.WarehouseId == warId && tempIn.NomId == row.NomId)
						{
							row.Separate[warsCounter] += tempIn.Amount;
							addedCounter++;
						}
					}

					/*if(removedCounter != woResult.Length)
					{
						var tempWo = woResult[removedCounter];
						if(tempWo.WarehouseId == warId && tempWo.NomId == row.NomId)
						{
							row.Separate[warsCounter] -= tempWo.Amount;
							removedCounter++;
						}
					}
				}

				AddRow(ref report, row);
			}*/

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
				ws.Cell(2, startIndex + i).InsertData(Report.SummaryRows.Select(sr => sr.Separate[i]));
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
				if(IsGreaterThanZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Separate[warCounter] > 0) == null
				|| IsLessOrEqualZeroByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Separate[warCounter] <= 0) == null
				|| IsLessThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min < row.Separate[warCounter]) == null
				|| IsGreaterOrEqualThanMinByWarehouse && report.SummaryRows.FirstOrDefault(row => row.Min >= row.Separate[warCounter]) == null)
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
			report.SummaryRows.ForEach(row => row.Separate.RemoveAt(i));
			warCounter--;
		}

		private void AddRow(ref BalanceSummaryReport report, BalanceSummaryRow row)
		{
			if(AllNomenclatures
				|| IsGreaterThanZeroByNomenclature && row.Separate.FirstOrDefault(war => war > 0) > 0
				|| IsLessOrEqualZeroByNomenclature && row.Separate.FirstOrDefault(war => war <= 0) <= 0
				|| IsLessThanMinByNomenclature && row.Separate.FirstOrDefault(war => war < row.Min) < row.Min
				|| IsGreaterOrEqualThanMinByNomenclature && row.Separate.FirstOrDefault(war => war >= row.Min) >= row.Min)
			{
				report.SummaryRows.Add(row);
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

	public class BalanceSummaryRow
	{
		public int NomId { get; set; }
		public string NomTitle { get; set; }
		public string InventoryNumber { get; set; }
		public decimal Min { get; set; }
		public decimal Common => Separate.Sum();
		public decimal Diff => Common - Min;
		public List<decimal> Separate { get; set; }
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
		public int WarehouseId { get; set; }
		public decimal Amount { get; set; }
		public decimal MinStockCount { get; set; }
	}
}

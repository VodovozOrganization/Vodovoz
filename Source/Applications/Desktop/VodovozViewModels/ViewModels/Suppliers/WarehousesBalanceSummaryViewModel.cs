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
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class WarehousesBalanceSummaryViewModel : DialogTabViewModelBase
	{
		private const string _xlsxFileFilter = "XLSX File (*.xlsx)";
		private readonly string _parameterNom = "nomenclatures";
		private readonly string _parameterNomType = "nomenclature_type";
		private readonly string _parameterProducGroups = "product_groups";
		private readonly string _parameterWarehouses = "warehouses";
		private readonly IFileDialogService _fileDialogService;
		private SelectableParameterReportFilterViewModel _nomsViewModel;
		private SelectableParameterReportFilterViewModel _warsViewModel;
		private SelectableParametersReportFilter _nomsFilter;
		private SelectableParametersReportFilter _warsFilter;

		private bool _isGenerating = false;
		private BalanceSummaryReport _report;
		private bool _isCreatedWithReserveData = false;

		public WarehousesBalanceSummaryViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation, IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_fileDialogService = fileDialogService;
			TabName = "Остатки по складам";
		}

		#region Свойства

		public DateTime? EndDate { get; set; } = DateTime.Today;
		public bool ShowReserve { get; set; }

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

		public SelectableParameterReportFilterViewModel NomsViewModel => _nomsViewModel ?? (_nomsViewModel = CreateNomsViewModel());

		public SelectableParameterReportFilterViewModel WarsViewModel => _warsViewModel ?? (_warsViewModel = CreateWarsViewModel());

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

			//Флаг типа отчета для экспорта в Эксель. Если выполнять проверку по ShowReserve,
			//то если после формирования отчета переключить чекбокс и нажать экспорт, отчет выгрузится неправильно
			_isCreatedWithReserveData = ShowReserve;

			endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);

			var nomsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNom);
			var noms = nomsSet?.GetIncludedParameters()?.ToList();
			var typesSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterNomType);
			var types = typesSet?.GetIncludedParameters()?.ToList();
			var groupsSet = _nomsFilter.ParameterSets.FirstOrDefault(x => x.ParameterName == _parameterProducGroups);
			var groups = groupsSet?.GetIncludedParameters()?.ToList();
			var wars = _warsFilter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == _parameterWarehouses)?.GetIncludedParameters()?.ToList();

			Nomenclature nomAlias = null;

			var warsIds = wars?.Select(x => (int)x.Value).ToArray();
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
				WarehousesTitles = wars?.Select(x => x.Title).ToList(),
				SummaryRows = new List<BalanceSummaryRow>()
			};

			#region Запросы

			WarehouseMovementOperation inAlias = null;
			WarehouseMovementOperation woAlias = null;
			BalanceBean resultAlias = null;

			var inQuery = localUow.Session.QueryOver(() => inAlias)
				.Where(() => inAlias.OperationTime <= endDate)
				.AndNot(() => nomAlias.IsArchive)
				.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
				.Where(Restrictions.In(Projections.Property(() => inAlias.IncomingWarehouse.Id), warsIds))
				.SelectList(list => list
					.SelectGroup(() => inAlias.IncomingWarehouse.Id).WithAlias(() => resultAlias.WarehouseId)
					.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
					.Select(Projections.Sum(Projections.Property(() => inAlias.Amount))).WithAlias(() => resultAlias.Amount)
				)
				.OrderBy(() => nomAlias.Id).Asc
				.ThenBy(() => inAlias.IncomingWarehouse.Id).Asc
				.TransformUsing(Transformers.AliasToBean<BalanceBean>());

			var woQuery = localUow.Session.QueryOver(() => woAlias)
				.Where(() => woAlias.OperationTime <= endDate)
				.AndNot(() => nomAlias.IsArchive)
				.Inner.JoinAlias(x => x.Nomenclature, () => nomAlias)
				.Where(Restrictions.In(Projections.Property(() => woAlias.WriteoffWarehouse.Id), warsIds))
				.SelectList(list => list
					.SelectGroup(() => woAlias.WriteoffWarehouse.Id).WithAlias(() => resultAlias.WarehouseId)
					.SelectGroup(() => nomAlias.Id).WithAlias(() => resultAlias.NomId)
					.Select(Projections.Sum(Projections.Property(() => woAlias.Amount))).WithAlias(() => resultAlias.Amount)
				)
				.OrderBy(() => nomAlias.Id).Asc
				.ThenBy(() => woAlias.WriteoffWarehouse.Id).Asc
				.TransformUsing(Transformers.AliasToBean<BalanceBean>());

			var msQuery = localUow.Session.QueryOver(() => nomAlias)
				.WhereNot(() => nomAlias.IsArchive)
				.Select(n => n.MinStockCount)
				.OrderBy(n => n.Id).Asc;

			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			ReservedBalance reservedBalance = null;
			ProductGroup productGroupAlias = null;

			OrderStatus[] orderStatusesToCalcReservedItems = new[] { OrderStatus.Accepted, OrderStatus.InTravelList, OrderStatus.OnLoading };

			var reservedItemsQuery = localUow.Session.QueryOver(() => orderAlias)
				.Where(Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), orderStatusesToCalcReservedItems))
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => nomAlias)
				.JoinAlias(() => nomAlias.ProductGroup, () => productGroupAlias)
				.Where(() => nomAlias.DoNotReserve == false)
				.Where(() => !nomAlias.IsArchive && !nomAlias.IsSerial);

			if(typesSelected)
			{
				var typesIds = types.Select(x => (int)x.Value).ToArray();
				inQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				woQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				msQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
				reservedItemsQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Category), typesIds));
			}

			if(nomsSelected && !allNomsSelected)
			{
				inQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				woQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				msQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
				reservedItemsQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.Id), nomsIds));
			}

			if(groupsSelected)
			{
				inQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				woQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				msQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
				reservedItemsQuery.Where(Restrictions.In(Projections.Property(() => nomAlias.ProductGroup.Id), groupsIds));
			}

			reservedItemsQuery
				.SelectList(list => list
					.SelectGroup(() => nomAlias.Id).WithAlias(() => reservedBalance.ItemId)
					.Select(Projections.Sum(() => orderItemsAlias.Count)).WithAlias(() => reservedBalance.ReservedItemsAmount))
				.TransformUsing(Transformers.AliasToBean<ReservedBalance>());

			#endregion

			var batch = localUow.Session.CreateQueryBatch()
				.Add<BalanceBean>("in", inQuery)
				.Add<BalanceBean>("wo", woQuery)
				.Add<decimal>("ms", msQuery);

			var inResult = batch.GetResult<BalanceBean>("in").ToArray();
			var woResult = batch.GetResult<BalanceBean>("wo").ToArray();
			var msResult = batch.GetResult<decimal>("ms").ToArray();

			List<ReservedBalance> reservedItems = new List<ReservedBalance>();
			if(ShowReserve)
			{
				reservedItems = reservedItemsQuery.List<ReservedBalance>().ToList();
			}

			//Кол-во списаний != кол-во начислений, используется два счетчика
			var addedCounter = 0;
			var removedCounter = 0;
			for(var nomsCounter = 0; nomsCounter < noms?.Count; nomsCounter++)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var row = new BalanceSummaryRow
				{
					NomId = (int)noms[nomsCounter].Value,
					NomTitle = noms[nomsCounter].Title,
					Separate = new List<decimal>(),
					Min = msResult[nomsCounter],
					ReservedItemsAmount = reservedItems
						.Where(i => i.ItemId == (int)noms[nomsCounter].Value)
						.Select(i => i.ReservedItemsAmount).FirstOrDefault() ?? 0
				};

				for(var warsCounter = 0; warsCounter < wars?.Count; warsCounter++)
				{
					row.Separate.Add(0);
					//Т.к. данные запросов упорядочены, тут реализован доступ по индексам
					var warId = (int)wars[warsCounter].Value;
					if(addedCounter != inResult.Length)
					{
						var tempIn = inResult[addedCounter];
						if(tempIn.WarehouseId == warId && tempIn.NomId == row.NomId)
						{
							row.Separate[warsCounter] += tempIn.Amount;
							addedCounter++;
						}
					}

					if(removedCounter != woResult.Length)
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

				if(_isCreatedWithReserveData)
				{
					InsertValuesWithReserveAmount(ws);
				}
				else
				{
					InsertValues(ws);
				}
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
			AddWarehouseCloumns(ws, index);
		}

		private void InsertValuesWithReserveAmount(IXLWorksheet ws)
		{
			var colNames = new string[] { "Код", "Наименование", "Мин. Остаток", "Резерв", "Доступно для заказа", "Общий остаток", "Разница" };
			var rows = from row in Report.SummaryRows
					   select new
					   {
						   row.NomId,
						   row.NomTitle,
						   row.Min,
						   row.ReservedItemsAmount,
						   row.AvailableItemsAmount,
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
			AddWarehouseCloumns(ws, index);
		}

		private void AddWarehouseCloumns(IXLWorksheet ws, int startIndex)
		{
			for(var i = 0; i < Report.WarehousesTitles.Count; i++)
			{
				ws.Cell(1, startIndex + i).Value = $"{Report.WarehousesTitles[i]}";
				ws.Cell(2, startIndex + i).InsertData(Report.SummaryRows.Select(sr => sr.Separate[i]));
			}
		}

		private void RemoveWarehousesByFilterCondition(ref BalanceSummaryReport report, CancellationToken cancellationToken)
		{
			if(AllWarehouses)
			{
				return;
			}

			for(var warCounter = 0; warCounter < report.WarehousesTitles.Count; warCounter++)
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
			report.WarehousesTitles.RemoveAt(warCounter);
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

			_nomsFilter.CreateParameterSet("Группы товаров", _parameterProducGroups,
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

		private SelectableParameterReportFilterViewModel CreateWarsViewModel()
		{
			_warsFilter = new SelectableParametersReportFilter(UoW);

			_warsFilter.CreateParameterSet("Склады", _parameterWarehouses, new ParametersFactory(UoW, (filters) =>
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
			}));

			return new SelectableParameterReportFilterViewModel(_warsFilter);
		}

		#endregion
	}

	public class BalanceSummaryRow
	{
		public int NomId { get; set; }
		public string NomTitle { get; set; }
		public decimal Min { get; set; }
		public decimal Common => Separate.Sum();
		public decimal Diff => Common - Min;
		public List<decimal> Separate { get; set; }
		public decimal? ReservedItemsAmount { get; set; } = 0;
		public decimal? AvailableItemsAmount => Common - ReservedItemsAmount;
	}

	public class BalanceSummaryReport
	{
		public DateTime EndDate { get; set; }
		public List<string> WarehousesTitles { get; set; }
		public List<BalanceSummaryRow> SummaryRows { get; set; }
	}

	public class BalanceBean
	{
		public int NomId { get; set; }
		public int WarehouseId { get; set; }
		public decimal Amount { get; set; }
	}

	public class ReservedBalance
	{
		public int ItemId { get; set; }
		public decimal? ReservedItemsAmount { get; set; }
	}
}

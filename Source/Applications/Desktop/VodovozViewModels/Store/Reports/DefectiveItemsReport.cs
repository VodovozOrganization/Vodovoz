using DateTimeHelpers;
using Gamma.Utilities;
using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.Presentation.ViewModels.Reports;
using VodovozBusiness.Domain.Documents;

namespace Vodovoz.ViewModels.Store.Reports
{
	[Appellative(Nominative = "Отчет по браку")]
	public partial class DefectiveItemsReport : IClosedXmlReport
	{
		private DefectiveItemsReport(
			DateTime startDate,
			DateTime endDate,
			int? driverId,
			DefectSource? defectSource,
			IEnumerable<DefectiveItemsReportRow> defectiveItemsReportRows,
			IEnumerable<SummaryRow> summaryRows,
			IEnumerable<SummaryDisplayRow> summaryDisplayRows,
			List<string> warehouseNames,
			List<SummaryBySourceRow> summaryBySourceRows)
		{
			StartDate = startDate;
			EndDate = endDate.LatestDayTime();
			DriverId = driverId;
			DefectSource = defectSource;
			Rows = defectiveItemsReportRows ?? throw new ArgumentNullException(nameof(defectiveItemsReportRows));
			SummaryRows = summaryRows ?? throw new ArgumentNullException(nameof(summaryRows));
			SummaryDisplayRows = summaryDisplayRows ?? throw new ArgumentNullException(nameof(summaryDisplayRows));
			WarehouseNames = warehouseNames ?? throw new ArgumentNullException(nameof(warehouseNames));
			SummaryBySourceRows = summaryBySourceRows;
			CreatedAt = DateTime.Now;
		}

		public string TemplatePath => @".\Reports\Store\DefectiveItemsReport.xlsx";

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public int? DriverId { get; }
		public DefectSource? DefectSource { get; }
		public DateTime CreatedAt { get; }

		public IEnumerable<DefectiveItemsReportRow> Rows { get; }
		public IEnumerable<SummaryRow> SummaryRows { get; }
		public IEnumerable<SummaryDisplayRow> SummaryDisplayRows { get; }
		public IEnumerable<SummaryBySourceRow> SummaryBySourceRows { get; }
		public List<string> WarehouseNames { get; }

		public static async Task<Result<DefectiveItemsReport>> Create(
			IUnitOfWork unitOfWork,
			DateTime startDate,
			DateTime endDate,
			DefectSource? defectSource,
			int? driverId,
			CancellationToken cancellationToken)
		{
			endDate = endDate.LatestDayTime();

			var regradingOfGoodsRows = await
				(from rogdi in unitOfWork.Session.Query<RegradingOfGoodsDocumentItem>()
				 join rogd in unitOfWork.Session.Query<RegradingOfGoodsDocument>()
				 on rogdi.Document.Id equals rogd.Id
				 where rogd.TimeStamp >= startDate
					&& rogd.TimeStamp <= endDate
					&& (defectSource == null || rogdi.Source == defectSource)
					&& driverId == null
					&& rogdi.TypeOfDefect != null
				 select new
				 {
					 DocumentId = rogd.Id,
					 rogd.TimeStamp,
					 WarehouseId = rogd.Warehouse.Id,
					 rogdi.Amount,
					 TypeOfDefectId = rogdi.TypeOfDefect.Id,
					 NomenclatureId = rogdi.NomenclatureNew.Id,
					 DefectSource = rogdi.Source,
					 DocumentType = typeof(RegradingOfGoodsDocument),
					 AuthorId = rogd.Author.Id,
					 rogd.Comment
				 })
				.ToListAsync(cancellationToken);

			var carUnloadDocumentRows = await
				(from cudi in unitOfWork.Session.Query<CarUnloadDocumentItem>()
				 join cud in unitOfWork.Session.Query<CarUnloadDocument>()
				 on cudi.Document.Id equals cud.Id
				 join gao in unitOfWork.Session.Query<GoodsAccountingOperation>()
				 on cudi.GoodsAccountingOperation.Id equals gao.Id
				 join rl in unitOfWork.Session.Query<RouteList>()
				 on cud.RouteList.Id equals rl.Id
				 where cud.TimeStamp >= startDate
					&& cud.TimeStamp <= endDate
					&& (defectSource == null || cudi.DefectSource == defectSource)
					&& (driverId == null || rl.Driver.Id == driverId)
					&& cudi.TypeOfDefect != null
				 select new
				 {
					 DocumentId = cud.Id,
					 cud.TimeStamp,
					 WarehouseId = cud.Warehouse.Id,
					 gao.Amount,
					 TypeOfDefectId = cudi.TypeOfDefect.Id,
					 NomenclatureId = gao.Nomenclature.Id,
					 cudi.DefectSource,
					 DriverId = rl.Driver.Id,
					 DocumentType = typeof(CarUnloadDocument),
					 RouteListId = rl.Id,
					 AuthorId = cud.Author.Id,
					 cud.Comment
				 })
				.ToListAsync(cancellationToken);

			var employeesIds = regradingOfGoodsRows
				.Select(x => x.AuthorId)
				.Concat(carUnloadDocumentRows.Select(x => x.DriverId))
				.Concat(carUnloadDocumentRows.Select(x => x.AuthorId))
				.Distinct()
				.ToArray();

			var employeesIdToFio =
				(from employee in unitOfWork.Session.Query<Employee>()
				 where employeesIds.Contains(employee.Id)
				 select new EmployeeIdToFioNode
				 {
					 Id = employee.Id,
					 Name = employee.Name,
					 LastName = employee.LastName,
					 Patronymic = employee.Patronymic
				 })
				.ToDictionary(x => x.Id);

			var warehousesIds = regradingOfGoodsRows
				.Select(x => x.WarehouseId)
				.Concat(carUnloadDocumentRows.Select(x => x.WarehouseId))
				.Distinct()
				.OrderBy(x => x)
				.ToArray();

			var warehousesIdsToNames =
				(from warehouse in unitOfWork.Session.Query<Warehouse>()
				 where warehousesIds.Contains(warehouse.Id)
				 select new EntityIdToNameNode
				 {
					 Id = warehouse.Id,
					 Name = warehouse.Name,
				 })
				.ToDictionary(x => x.Id);

			var nomenclatureIds = regradingOfGoodsRows
				.Select(x => x.NomenclatureId)
				.Concat(carUnloadDocumentRows.Select(x => x.NomenclatureId))
				.Distinct()
				.ToArray();

			var nomenclatureIdsNames =
				(from nomenclature in unitOfWork.Session.Query<Nomenclature>()
				 where nomenclatureIds.Contains(nomenclature.Id)
				 select new EntityIdToNameNode
				 {
					 Id = nomenclature.Id,
					 Name = nomenclature.Name,
				 })
				.ToDictionary(x => x.Id);

			var defectTypesIds = regradingOfGoodsRows
				.Select(x => x.TypeOfDefectId)
				.Concat(carUnloadDocumentRows.Select(x => x.TypeOfDefectId))
				.Distinct()
				.ToArray();

			var defectTypesIdsNames =
				(from cullingCategory in unitOfWork.Session.Query<CullingCategory>()
				 where defectTypesIds.Contains(cullingCategory.Id)
				 select new EntityIdToNameNode
				 {
					 Id = cullingCategory.Id,
					 Name = cullingCategory.Name,
				 })
				.ToDictionary(x => x.Id);

			var rows = new List<DefectiveItemsReportRow>();

			foreach(var regradingOfGoodsRow in regradingOfGoodsRows)
			{
				rows.Add(new DefectiveItemsReportRow
				{
					Id = regradingOfGoodsRow.DocumentId,
					Date = regradingOfGoodsRow.TimeStamp,
					Amount = regradingOfGoodsRow.Amount,
					WarehouseId = regradingOfGoodsRow.WarehouseId,
					AuthorLastName = employeesIdToFio[regradingOfGoodsRow.AuthorId].LastName,
					DefectiveItemName = nomenclatureIdsNames[regradingOfGoodsRow.NomenclatureId].Name,
					DefectSource = regradingOfGoodsRow.DefectSource,
					DriverLastName = "",
					DocumentType = regradingOfGoodsRow.DocumentType,
					DefectTypeId = regradingOfGoodsRow.TypeOfDefectId,
					DefectTypeName = defectTypesIdsNames[regradingOfGoodsRow.TypeOfDefectId].Name,
					RouteListId = null,
					Comment = regradingOfGoodsRow.Comment,
					DefectDetectedAt = warehousesIdsToNames[regradingOfGoodsRow.WarehouseId].Name,
				});
			}

			foreach(var carUnloadDucomentRow in carUnloadDocumentRows)
			{
				rows.Add(new DefectiveItemsReportRow
				{
					Id = carUnloadDucomentRow.DocumentId,
					Date = carUnloadDucomentRow.TimeStamp,
					Amount = carUnloadDucomentRow.Amount,
					WarehouseId = carUnloadDucomentRow.WarehouseId,
					AuthorLastName = employeesIdToFio[carUnloadDucomentRow.AuthorId].LastName,
					DefectiveItemName = nomenclatureIdsNames[carUnloadDucomentRow.NomenclatureId].Name,
					DefectSource = carUnloadDucomentRow.DefectSource,
					DriverLastName = employeesIdToFio[carUnloadDucomentRow.DriverId].LastName,
					DocumentType = carUnloadDucomentRow.DocumentType,
					DefectTypeId = carUnloadDucomentRow.TypeOfDefectId,
					DefectTypeName = defectTypesIdsNames[carUnloadDucomentRow.TypeOfDefectId].Name,
					RouteListId = carUnloadDucomentRow.RouteListId,
					Comment = carUnloadDucomentRow.Comment,
					DefectDetectedAt = warehousesIdsToNames[carUnloadDucomentRow.WarehouseId].Name,
				});
			}

			var sortedRows = rows.OrderBy(x => x.Date)
				.ThenBy(x => x.DefectSource)
				.ThenBy(x => x.DefectTypeName)
				.ToList();

			var summaryRows = new List<SummaryRow>();
			var summaryDisplayRows = new List<SummaryDisplayRow>();

			var warehouseNames = warehousesIdsToNames.Values
				.OrderBy(x => x.Id)
				.Select(x => x.Name)
				.ToList();

			foreach (var defectTypeId in defectTypesIds)
			{
				var amountBySource = new List<decimal>();

				foreach(var warehouseId in warehousesIds)
				{
					amountBySource.Add(
						sortedRows
							.Where(x => x.DefectTypeId == defectTypeId && x.WarehouseId == warehouseId)
							.Sum(x => x.Amount));
				}

				summaryRows.Add(new SummaryRow
				{
					DefectType = defectTypesIdsNames[defectTypeId].Name,
					DynamicColls = amountBySource,
					Summary = amountBySource.Sum(x => x)
				});

				summaryDisplayRows.Add(new SummaryDisplayRow
				{
					Title = defectTypesIdsNames[defectTypeId].Name,
					DynamicColls = amountBySource.Select(x => x.ToString("# ##0")),
					Summary = amountBySource.Sum(x => x).ToString("# ##0")
				});
			}

			var summaryByColumn = new List<decimal>();

			for(var i = 0; i < warehousesIds.Length; i++)
			{
				var currentSum = 0m;

				foreach(var row in summaryRows)
				{
					currentSum += row.DynamicColls.ElementAt(i);
				}

				summaryByColumn.Add(currentSum);
			}

			summaryDisplayRows.Add(new SummaryDisplayRow()
			{
				Title = "Итог",
				DynamicColls = summaryByColumn.Select(x => x.ToString("# ##0")),
				Summary = summaryByColumn.Sum().ToString("# ##0")
			});

			var summaryBySourceRows = new List<SummaryBySourceRow>();

			foreach(DefectSource source in Enum.GetValues(typeof(DefectSource)))
			{
				summaryBySourceRows.Add(new SummaryBySourceRow()
				{
					Title = source.GetEnumTitle(),
					Value = sortedRows
						.Where(x => x.DefectSource == source)
						.Sum(x => x.Amount)
						.ToString("# ##0")
				});
			}

			summaryBySourceRows.RemoveAll(x => x.Value.Trim() == "0");

			return new DefectiveItemsReport(startDate, endDate, driverId, defectSource, sortedRows, summaryRows, summaryDisplayRows, warehouseNames, summaryBySourceRows);
		}
	}
}

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
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Extensions;
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
			IEnumerable<SummaryDisplayRow> summaryDisplayRows,
			List<string> warehouseNames,
			List<SummaryBySourceRow> summaryBySourceRows,
			List<string> sourceNames,
			List<string> defectNames,
			IEnumerable<SummaryByNomenclatureRow> summaryByNomenclatureRow,
			IEnumerable<SummaryByNomenclatureWithTypeDefectRow> summaryByNomenclatureWithTypeDefectRow)
		{
			StartDate = startDate;
			EndDate = endDate.LatestDayTime();
			DriverId = driverId;
			DefectSource = defectSource;
			Rows = defectiveItemsReportRows ?? throw new ArgumentNullException(nameof(defectiveItemsReportRows));
			SummaryDisplayRows = summaryDisplayRows;
			WarehouseNames = warehouseNames;
			SummaryBySourceRows = summaryBySourceRows;
			SourceNames = sourceNames;
			DefectNames = defectNames;
			SummaryByNomenclatureRows = summaryByNomenclatureRow;
			SummaryByNomenclatureWithTypeDefectRows = summaryByNomenclatureWithTypeDefectRow;
			CreatedAt = DateTime.Now;

			TemplatePath = summaryDisplayRows == null 
				? @".\Reports\Store\DefectiveItemsReportForOneWarehouse.xlsx"
				: @".\Reports\Store\DefectiveItemsReport.xlsx";
		}

		public string TemplatePath { get; set; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public int? DriverId { get; }
		public DefectSource? DefectSource { get; }
		public DateTime CreatedAt { get; }

		public IEnumerable<DefectiveItemsReportRow> Rows { get; }
		public IEnumerable<SummaryDisplayRow> SummaryDisplayRows { get; }
		public IEnumerable<SummaryBySourceRow> SummaryBySourceRows { get; }
		public IEnumerable<SummaryByNomenclatureRow> SummaryByNomenclatureRows { get; }
		public IEnumerable<SummaryByNomenclatureWithTypeDefectRow> SummaryByNomenclatureWithTypeDefectRows { get; }
		public List<string> WarehouseNames { get; }
		public List<string> SourceNames { get; }
		public List<string> DefectNames { get; }


		public static async Task<Result<DefectiveItemsReport>> Create(
			IUnitOfWork unitOfWork,
			DateTime startDate,
			DateTime endDate,
			DefectSource? defectSource,
			int? driverId,
			CancellationToken cancellationToken,
			Warehouse selectedWarehouse = null)
		{
			endDate = endDate.LatestDayTime();

			if(selectedWarehouse == null)
			{
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
							AuthorId = rogd.AuthorId.Value,
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
							AuthorId = cud.AuthorId.Value,
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
						NomenclatureId = regradingOfGoodsRow.NomenclatureId,
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
						NomenclatureId = carUnloadDucomentRow.NomenclatureId,
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

				foreach(var defectTypeId in defectTypesIds)
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

				return new DefectiveItemsReport(startDate, endDate, driverId, defectSource, sortedRows, summaryDisplayRows,
					warehouseNames, summaryBySourceRows, null, null, null, null);

			}
			else
			{
				var regradingOfGoodsRows = await
					(from rogdi in unitOfWork.Session.Query<RegradingOfGoodsDocumentItem>()
						join rogd in unitOfWork.Session.Query<RegradingOfGoodsDocument>()
							on rogdi.Document.Id equals rogd.Id
						where rogd.TimeStamp >= startDate
						      && rogd.TimeStamp <= endDate
						      && (defectSource == null || rogdi.Source == defectSource)
						      && driverId == null
						      && rogdi.TypeOfDefect != null
						      && rogd.Warehouse.Id == selectedWarehouse.Id
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
							AuthorId = rogd.AuthorId.Value,
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
						      && cud.Warehouse.Id == selectedWarehouse.Id
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
							AuthorId = cud.AuthorId.Value,
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

				var warehousesIds = new[] { selectedWarehouse.Id };

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
					.ToArray()
					.OrderBy(x => x);

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
						NomenclatureId = regradingOfGoodsRow.NomenclatureId,
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
						NomenclatureId = carUnloadDucomentRow.NomenclatureId,
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
				
				var sourceNames =
					(from DefectSource source in Enum.GetValues(typeof(DefectSource))
						select source.GetEnumDisplayName())
					.ToList();
				
				var summaryByNomenclatureRows = nomenclatureIds
					.Select(nomenclatureId => new
					{
						nomenclatureId,
						amountBySource =
							(from DefectSource source in Enum.GetValues(typeof(DefectSource))
								select sortedRows.Where(x => x.DefectSource == source && x.NomenclatureId == nomenclatureId)
									.Sum(x => x.Amount)).ToList()
					})
					.Select(t => new SummaryByNomenclatureRow()
					{
						NomeclatureName = nomenclatureIdsNames[t.nomenclatureId].Name,
						DynamicColumns = t.amountBySource.Select(x => x.ToString("# ##0")),
						DynamicColumnsValue = t.amountBySource
					})
					.ToList();
				
				var summaryByColumn = new List<decimal>();
				for(var i = 0; i <  Enum.GetValues(typeof(DefectSource)).Length; i++)
				{
					var currentSum = summaryByNomenclatureRows.Sum(row => row.DynamicColumnsValue.ElementAt(i));
					summaryByColumn.Add(currentSum);
				}
				summaryByNomenclatureRows.Add(new SummaryByNomenclatureRow()
				{
					NomeclatureName = "Итог",
					DynamicColumns = summaryByColumn.Select(x => x.ToString("# ##0")),
				});

				var defectNames = defectTypesIdsNames.Values
					.OrderBy(x => x.Id)
					.Select(x => x.Name)
					.ToList();
				
				var summaryByNomenclatureWithTypeDefectRows = nomenclatureIds
					.Select(nomenclatureId => new
					{
						nomenclatureId,
						amountByDefect =
							defectTypesIds.Select(defectTypesId =>
									sortedRows.Where(x => x.DefectTypeId == defectTypesId && x.NomenclatureId == nomenclatureId)
										.Sum(x => x.Amount))
								.ToList()
					})
					.Select(t => new SummaryByNomenclatureWithTypeDefectRow()
					{
						NomeclatureName = nomenclatureIdsNames[t.nomenclatureId].Name,
						DynamicColumns = t.amountByDefect.Select(x => x.ToString("# ##0")),
					})
					.ToList();

				return new DefectiveItemsReport(startDate, endDate, driverId, defectSource, sortedRows, null, null,
					null, sourceNames, defectNames, summaryByNomenclatureRows, summaryByNomenclatureWithTypeDefectRows);
			}
		}
		
	}
}

using ClosedXML.Excel;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.EntityRepositories.Sale;
using VodovozBusiness.Nodes.SalesReport;

namespace Vodovoz.ViewModels.Services.SalesReport
{
	public class SalesReportService : ISalesReportService
	{
		private readonly ISalesReportRepository _salesReportRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public SalesReportService(
			ISalesReportRepository salesReportRepository,
			INomenclatureSettings nomenclatureSettings
			)
		{
			_salesReportRepository = salesReportRepository ?? throw new ArgumentNullException(nameof(salesReportRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		public async Task<IList<SalesReportDataNode>> GetSalesReportDataAsync(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			OrderDateFilterType orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken = default)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			return await _salesReportRepository.GetSalesReportData(uow, startDate, endDate, orderDateType, filters, cancellationToken);
		}

		public async Task<BottlesDataNode> GetBottlesDataAsync(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			CancellationToken cancellationToken = default)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(orderIds is null || !orderIds.Any())
			{
				return new BottlesDataNode
				{
					Plan = 0,
					FactFromRouteList = 0,
					FactFromSelfDelivery = 0
				};
			}

			return await _salesReportRepository.GetBottlesData(
				uow,
				orderIds,
				_nomenclatureSettings.DefaultBottleNomenclatureId,
				cancellationToken);
		}

		public byte[] ExportToExcel(
			IList<SalesReportTreeNode> tree,
			DateTime startDate,
			DateTime endDate,
			string groupingTitle,
			int ordersCount,
			int planBottles,
			int factBottles)
		{
			if(tree == null || !tree.Any())
			{
				throw new ArgumentException("Нет данных для экспорта", nameof(tree));
			}

			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Отчет по продажам");

				int currentRow = 1;

				var titleCell = worksheet.Cell(currentRow, 1);
				titleCell.Value = $"Отчет по продажам за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
				titleCell.Style.Font.FontSize = 14;
				titleCell.Style.Font.Bold = true;
				worksheet.Range(currentRow, 1, currentRow, 7).Merge();
				currentRow++;

				var groupingCell = worksheet.Cell(currentRow, 1);
				groupingCell.Value = $"Группировка: {groupingTitle}";
				groupingCell.Style.Font.Italic = true;
				worksheet.Range(currentRow, 1, currentRow, 7).Merge();
				currentRow++;

				currentRow++;

				worksheet.Cell(currentRow, 1).Value = "Код";
				worksheet.Cell(currentRow, 2).Value = "Клиент";
				worksheet.Cell(currentRow, 3).Value = "Точка доставки";
				worksheet.Cell(currentRow, 4).Value = "Заказ/Дата/Автор";
				worksheet.Cell(currentRow, 5).Value = "Номенклатура";
				worksheet.Cell(currentRow, 6).Value = "Кол-во";
				worksheet.Cell(currentRow, 7).Value = "Сумма";

				var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
				headerRange.Style.Font.Bold = true;
				headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

				worksheet.Column(1).Width = 10;
				worksheet.Column(2).Width = 35;
				worksheet.Column(3).Width = 50;
				worksheet.Column(4).Width = 25;
				worksheet.Column(5).Width = 45;
				worksheet.Column(6).Width = 12;
				worksheet.Column(7).Width = 18;

				worksheet.Column(4).Style.Alignment.WrapText = true;
				worksheet.Column(5).Style.Alignment.WrapText = true;

				currentRow++;

				FillExcelRows(worksheet, tree, ref currentRow);

				currentRow++;

				var totalCount = tree.Sum(n => n.TotalCount);
				var totalSum = tree.Sum(n => n.TotalSum);

				var totalRange = worksheet.Range(currentRow, 1, currentRow, 5);
				totalRange.Merge();
				worksheet.Cell(currentRow, 1).Value = "Итого:";
				worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

				worksheet.Cell(currentRow, 6).Value = totalCount;
				worksheet.Cell(currentRow, 6).Style.Font.Bold = true;
				worksheet.Cell(currentRow, 7).Value = totalSum;
				worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

				currentRow++;
				currentRow++;

				var ordersCell = worksheet.Cell(currentRow, 1);
				ordersCell.Value = $"Количество заказов: {ordersCount}";
				ordersCell.Style.Font.Italic = true;
				worksheet.Range(currentRow, 1, currentRow, 7).Merge();
				currentRow++;

				var factBottlesCell = worksheet.Cell(currentRow, 1);
				factBottlesCell.Value = $"Фактически забранная тара: {factBottles}";
				factBottlesCell.Style.Font.Italic = true;
				worksheet.Range(currentRow, 1, currentRow, 7).Merge();
				currentRow++;

				var planBottlesCell = worksheet.Cell(currentRow, 1);
				planBottlesCell.Value = $"Планируемая тара: {planBottles}";
				planBottlesCell.Style.Font.Italic = true;
				worksheet.Range(currentRow, 1, currentRow, 7).Merge();

				using(var stream = new MemoryStream())
				{
					workbook.SaveAs(stream);
					return stream.ToArray();
				}
			}
		}

		private void FillExcelRows(
			IXLWorksheet worksheet,
			IEnumerable<SalesReportTreeNode> nodes,
			ref int currentRow)
		{
			foreach(var node in nodes)
			{
				if(node.Children != null && node.Children.Any())
				{
					var range = worksheet.Range(currentRow, 1, currentRow, 5);
					range.Merge();
					worksheet.Cell(currentRow, 1).Value = node.Name;
					worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

					worksheet.Cell(currentRow, 6).Value = node.TotalCount;
					worksheet.Cell(currentRow, 7).Value = node.TotalSum;

					var parentRange = worksheet.Range(currentRow, 1, currentRow, 7);
					parentRange.Style.Font.Bold = true;

					currentRow++;

					FillExcelRows(worksheet, node.Children, ref currentRow);
				}
				else if(node.Data != null)
				{
					var data = node.Data;

					worksheet.Cell(currentRow, 1).Value = data.CounterpartyId;
					worksheet.Cell(currentRow, 2).Value = data.Counterparty;
					worksheet.Cell(currentRow, 3).Value = data.DeliveryPoint;
					worksheet.Cell(currentRow, 4).Value = data.OrdDetails;
					worksheet.Cell(currentRow, 5).Value = data.NomenclatureName;
					worksheet.Cell(currentRow, 6).Value = data.TotalCount;
					worksheet.Cell(currentRow, 7).Value = data.TotalSum;

					currentRow++;
				}
			}
		}
	}
}

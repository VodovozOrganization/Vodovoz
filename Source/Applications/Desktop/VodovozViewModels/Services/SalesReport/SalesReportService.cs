using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.EntityRepositories.Sale;
using VodovozBusiness.Errors.Reports;
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
			
			if(filters is null)
			{
				throw new ArgumentNullException(nameof(filters));
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
					Fact = 0
				};
			}

			return await _salesReportRepository.GetBottlesData(
				uow,
				orderIds,
				_nomenclatureSettings.DefaultBottleNomenclatureId,
				cancellationToken);
		}

		public Result ExportToExcel(
			IList<SalesReportTreeNode> tree,
			DateTime startDate,
			DateTime endDate,
			string groupingTitle,
			int ordersCount,
			int planBottles,
			int factBottles,
			string outputFilePath,
			bool showPhones)
		{
			if(tree is null || !tree.Any())
			{
				return Result.Failure(SalesReportErrors.NoDataForExport);
			}

			if(string.IsNullOrWhiteSpace(outputFilePath))
			{
				return Result.Failure(SalesReportErrors.InvalidFilePath);
			}

			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Отчет по продажам");

				int currentRow = 1;
				int headerRow = 0;
				int totalRow = 0;
				var phoneColumnOffset = showPhones ? 1 : 0;
				var columnCount = showPhones ? 8 : 7;

				var titleCell = worksheet.Cell(currentRow, 1);
				titleCell.Value = $"Отчет по продажам за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
				titleCell.Style.Font.FontSize = 14;
				titleCell.Style.Font.Bold = true;
				titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
				worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();
				currentRow++;

				var groupingCell = worksheet.Cell(currentRow, 1);
				groupingCell.Value = $"Группировка: {groupingTitle}";
				groupingCell.Style.Font.Italic = true;
				groupingCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				groupingCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
				worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();
				currentRow++;
				currentRow++;

				headerRow = currentRow;
				worksheet.Cell(currentRow, 1).Value = "Код";
				worksheet.Cell(currentRow, 2).Value = "Клиент";
				worksheet.Cell(currentRow, 3).Value = "Точка доставки";
				worksheet.Cell(currentRow, 4).Value = "Заказ/Дата/Автор";

				if(showPhones)
				{
					worksheet.Cell(currentRow, 5).Value = "Телефоны";
				}

				worksheet.Cell(currentRow, 5 + phoneColumnOffset).Value = "Номенклатура";
				worksheet.Cell(currentRow, 6 + phoneColumnOffset).Value = "Кол-во";
				worksheet.Cell(currentRow, 7 + phoneColumnOffset).Value = "Сумма";

				var headerRange = worksheet.Range(currentRow, 1, currentRow, columnCount);
				headerRange.Style.Font.Bold = true;
				headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
				headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

				worksheet.Column(1).Width = 10;
				worksheet.Column(2).Width = 35;
				worksheet.Column(3).Width = 50;
				worksheet.Column(4).Width = 25;

				if(showPhones)
				{
					worksheet.Column(5).Width = 20;
				}

				worksheet.Column(5 + phoneColumnOffset).Width = 45;
				worksheet.Column(6 + phoneColumnOffset).Width = 12;
				worksheet.Column(7 + phoneColumnOffset).Width = 18;

				worksheet.Column(3).Style.Alignment.WrapText = true;
				worksheet.Column(3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
				worksheet.Column(4).Style.Alignment.WrapText = true;
				worksheet.Column(4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
				worksheet.Column(5 + phoneColumnOffset).Style.Alignment.WrapText = true;

				if(showPhones)
				{
					worksheet.Column(5).Style.Alignment.WrapText = true;
					worksheet.Column(5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
				}

				currentRow++;

				var startDataRow = currentRow;
				FillExcelRows(worksheet, tree, ref currentRow, showPhones, phoneColumnOffset);
				var endDataRow = currentRow - 1;

				totalRow = currentRow;
				var totalCount = tree.Sum(n => n.TotalCount);
				var totalSum = tree.Sum(n => n.TotalSum);

				var totalMergeRange = worksheet.Range(currentRow, 1, currentRow, 5 + phoneColumnOffset);
				totalMergeRange.Merge();

				worksheet.Cell(currentRow, 1).Value = "Итого:";
				worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
				worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

				worksheet.Cell(currentRow, 6 + phoneColumnOffset).Value = totalCount;
				worksheet.Cell(currentRow, 6 + phoneColumnOffset).Style.Font.Bold = true;
				worksheet.Cell(currentRow, 6 + phoneColumnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				worksheet.Cell(currentRow, 6 + phoneColumnOffset).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

				worksheet.Cell(currentRow, 7 + phoneColumnOffset).Value = totalSum;
				worksheet.Cell(currentRow, 7 + phoneColumnOffset).Style.Font.Bold = true;
				worksheet.Cell(currentRow, 7 + phoneColumnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				worksheet.Cell(currentRow, 7 + phoneColumnOffset).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

				currentRow++;

				if(startDataRow <= endDataRow)
				{
					var tableRange = worksheet.Range(headerRow, 1, totalRow, columnCount);
					tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
					tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
				}

				var usedRows = worksheet.RowsUsed().ToList();
				foreach(var row in usedRows)
				{
					if(row.RowNumber() >= headerRow)
					{
						var maxLineCount = 1;

						var cell3Value = row.Cell(3).GetString();
						if(!string.IsNullOrWhiteSpace(cell3Value))
						{
							var maxLineLength = worksheet.Column(3).Width;
							var lines = cell3Value.Split('\n');
							var lineCount = lines.Length;

							foreach(var line in lines)
							{
								var approxLineCount = (int)Math.Ceiling((double)line.Length / maxLineLength);
								lineCount += approxLineCount - 1;
							}
							maxLineCount = Math.Max(maxLineCount, lineCount);
						}

						var cell4Value = row.Cell(4).GetString();
						if(!string.IsNullOrWhiteSpace(cell4Value))
						{
							var maxLineLength = worksheet.Column(4).Width;
							var lines = cell4Value.Split('\n');
							var lineCount = lines.Length;

							foreach(var line in lines)
							{
								var approxLineCount = (int)Math.Ceiling((double)line.Length / maxLineLength);
								lineCount += approxLineCount - 1;
							}
							maxLineCount = Math.Max(maxLineCount, lineCount);
						}

						var newHeight = Math.Max(15, maxLineCount * 15);
						row.Height = newHeight;
					}
				}

				currentRow++;
				var ordersCell = worksheet.Cell(currentRow, 1);
				ordersCell.Value = $"Количество заказов: {ordersCount}";
				ordersCell.Style.Font.Italic = true;
				ordersCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
				worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();
				currentRow++;

				var factBottlesCell = worksheet.Cell(currentRow, 1);
				factBottlesCell.Value = $"Фактически забранная тара: {factBottles}";
				factBottlesCell.Style.Font.Italic = true;
				factBottlesCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
				worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();
				currentRow++;

				var planBottlesCell = worksheet.Cell(currentRow, 1);
				planBottlesCell.Value = $"Планируемая тара: {planBottles}";
				planBottlesCell.Style.Font.Italic = true;
				planBottlesCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
				worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();

				workbook.SaveAs(outputFilePath);

				return Result.Success();
			}
		}

		private void FillExcelRows(
			IXLWorksheet worksheet,
			IEnumerable<SalesReportTreeNode> nodes,
			ref int currentRow,
			bool showPhones,
			int phoneColumnOffset)
		{
			foreach(var node in nodes)
			{
				if(node.Children != null && node.Children.Any())
				{
					var mergeEndColumn = 5 + phoneColumnOffset;
					var mergeRange = worksheet.Range(currentRow, 1, currentRow, mergeEndColumn);
					mergeRange.Merge();

					worksheet.Cell(currentRow, 1).Value = node.Name;
					worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
					worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
					worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

					worksheet.Cell(currentRow, 6 + phoneColumnOffset).Value = node.TotalCount;
					worksheet.Cell(currentRow, 6 + phoneColumnOffset).Style.Font.Bold = true;
					worksheet.Cell(currentRow, 6 + phoneColumnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 6 + phoneColumnOffset).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					worksheet.Cell(currentRow, 7 + phoneColumnOffset).Value = node.TotalSum;
					worksheet.Cell(currentRow, 7 + phoneColumnOffset).Style.Font.Bold = true;
					worksheet.Cell(currentRow, 7 + phoneColumnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 7 + phoneColumnOffset).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					currentRow++;

					FillExcelRows(worksheet, node.Children, ref currentRow, showPhones, phoneColumnOffset);
				}
				else if(node.Data != null)
				{
					var data = node.Data;

					worksheet.Cell(currentRow, 1).Value = data.CounterpartyId;
					worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					worksheet.Cell(currentRow, 2).Value = data.Counterparty;
					worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					worksheet.Cell(currentRow, 3).Value = data.DeliveryPoint;
					worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					worksheet.Cell(currentRow, 4).Value = data.OrdDetails;
					worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					if(showPhones)
					{
						worksheet.Cell(currentRow, 5).Value = data.Phones;
						worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
						worksheet.Cell(currentRow, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
					}

					worksheet.Cell(currentRow, 5 + phoneColumnOffset).Value = data.NomenclatureName;
					worksheet.Cell(currentRow, 5 + phoneColumnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 5 + phoneColumnOffset).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					worksheet.Cell(currentRow, 6 + phoneColumnOffset).Value = data.TotalCount;
					worksheet.Cell(currentRow, 6 + phoneColumnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 6 + phoneColumnOffset).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					worksheet.Cell(currentRow, 7 + phoneColumnOffset).Value = data.TotalSum;
					worksheet.Cell(currentRow, 7 + phoneColumnOffset).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					worksheet.Cell(currentRow, 7 + phoneColumnOffset).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

					currentRow++;
				}
			}
		}

		public Result ExportToPdf(
			IList<SalesReportTreeNode> tree,
			DateTime startDate,
			DateTime endDate,
			string groupingTitle,
			int ordersCount,
			int planBottles,
			int factBottles,
			string outputFilePath,
			bool showPhones)
		{
			var tempExcelPath = Path.GetTempFileName() + ".xlsx";

			try
			{
				var excelResult = ExportToExcel(
					tree, startDate, endDate, groupingTitle,
					ordersCount, planBottles, factBottles,
					tempExcelPath, showPhones);

				if(!excelResult.IsSuccess)
				{
					return excelResult;
				}

				ConvertExcelToPdf(tempExcelPath, outputFilePath);

				return Result.Success();
			}
			catch(Exception ex)
			{
				return Result.Failure(new Error("PdfExportError", ex.Message));
			}
			finally
			{
				if(File.Exists(tempExcelPath))
				{
					File.Delete(tempExcelPath);
				}
			}
		}

		private void ConvertExcelToPdf(string excelPath, string pdfPath)
		{
			using(var existingWorkbook = new XLWorkbook(excelPath))
			{
				var worksheet = existingWorkbook.Worksheet(1);

				using(var memoryStream = new MemoryStream())
				{
					var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
					var writer = PdfWriter.GetInstance(document, memoryStream);
					document.Open();

					string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
					var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
					var normalFont = new Font(baseFont, 9, Font.NORMAL);
					var boldFont = new Font(baseFont, 9, Font.BOLD);

					var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
					var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

					var table = new PdfPTable(lastColumn)
					{
						WidthPercentage = 100
					};

					for(int row = 1; row <= lastRow; row++)
					{
						for(int col = 1; col <= lastColumn; col++)
						{
							var currentCell = worksheet.Cell(row, col);

							if(currentCell.IsMerged())
							{
								var mergeRange = currentCell.MergedRange();
								if(mergeRange != null)
								{
									if(mergeRange.FirstColumn().ColumnNumber() != col ||
									   mergeRange.FirstRow().RowNumber() != row)
									{
										continue;
									}
								}
							}

							var cellValue = currentCell.GetString();
							var isBold = currentCell.Style.Font.Bold;
							var font = isBold ? boldFont : normalFont;
							var alignment = GetAlignment(currentCell.Style.Alignment.Horizontal);

							var pdfCell = new PdfPCell(new Phrase(cellValue, font))
							{
								HorizontalAlignment = alignment,
								VerticalAlignment = Element.ALIGN_MIDDLE,
								Padding = 4
							};

							if(currentCell.IsMerged())
							{
								var mergeRange = currentCell.MergedRange();
								if(mergeRange != null)
								{
									var colspan = mergeRange.ColumnCount();
									if(colspan > 1)
									{
										pdfCell.Colspan = colspan;
									}

									var rowspan = mergeRange.RowCount();
									if(rowspan > 1)
									{
										pdfCell.Rowspan = rowspan;
									}
								}
							}

							table.AddCell(pdfCell);
						}
					}

					document.Add(table);
					document.Close();

					File.WriteAllBytes(pdfPath, memoryStream.ToArray());
				}
			}
		}

		private int GetAlignment(XLAlignmentHorizontalValues alignment)
		{
			switch(alignment)
			{
				case XLAlignmentHorizontalValues.Center:
					return Element.ALIGN_CENTER;
				case XLAlignmentHorizontalValues.Right:
					return Element.ALIGN_RIGHT;
				default:
					return Element.ALIGN_LEFT;
			}
		}
	}
}

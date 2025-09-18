using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Project.Services.FileDialog;
using RestSharp.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales.RetailSalesReportFor1c
{
	/// <summary>
	/// Отчёт о розничных продажах для 1С
	/// </summary>
	[Appellative(Nominative = "Отчёт о розничных продажах для 1С")]
	public partial class RetailSalesReportFor1c
	{
		private const double _defaultColumnWidth = 12;
		private uint _defaultCellFormatId;
		private uint _tableHeadersCellFormatId;
		private uint _horizontalLineMiddleFormatId;
		private uint _simpleBoldTextFormat;
		private uint _smallFontFormat;
		private uint _simpleTextFormat;
		private uint _horizontalLineThinFormatId;


		public void Generate(IList<Order> orders, IProgressBarDisplayable progressBarDisplayable, CancellationToken token)
		{
			var ordersCount = orders.Count;

			progressBarDisplayable.Start(ordersCount, 0, "Выгрузка розницы");

			var rows = new List<RetailSalesReportFor1cRow>();

			var groupedRowsDict = new Dictionary<(string Code1c, decimal Price), RetailSalesReportFor1cRow>();

			for(int i = 0; i < ordersCount; i++)
			{
				token.ThrowIfCancellationRequested();

				foreach(var item in orders[i].OrderItems)
				{
					var key = (item.Nomenclature.Code1c, item.Price);

					if(groupedRowsDict.TryGetValue(key, out var existingRow))
					{
						existingRow.Amount += item.CurrentCount;
						existingRow.Sum += item.Sum;
						existingRow.Nds += item.CurrentNDS;
					}
					else
					{
						groupedRowsDict[key] = new RetailSalesReportFor1cRow
						{
							Amount = item.CurrentCount,
							Mesure = item.Nomenclature.Unit.Name,
							Code1c = item.Nomenclature.Code1c,
							Nomenclature = item.Nomenclature.Name,
							Price = item.Price,
							Sum = item.Sum,
							Nds = item.CurrentNDS
						};
					}
				}

				progressBarDisplayable.Add(1, $"Обработка заказа {i + 1}/{ordersCount}");
			}

			Rows = groupedRowsDict.Values.OrderBy(x => x.Nomenclature).ToList();

			progressBarDisplayable.Update("Выгрузка отчёта по рознице завершена. Сохранение в файл...");
		}

		public void SaveReport(IFileDialogService fileDialogService, string organizationINN)
		{
			var fileName = $"{GetType().GetAttribute<AppellativeAttribute>().Nominative} ИНН {organizationINN}";

			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{Title}.xlsx";

			var saveDialogResult = fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!saveDialogResult.Successful)
			{
				return;
			}

			using(var spreadsheet = SpreadsheetDocument.Create(saveDialogResult.Path, SpreadsheetDocumentType.Workbook))
			{
				spreadsheet.AddWorkbookPart();
				spreadsheet.WorkbookPart.Workbook = new Workbook();

				var worksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
				worksheetPart.Worksheet = new Worksheet();

				var stylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();
				stylesPart.Stylesheet = GetStyleSheet();
				stylesPart.Stylesheet.Save();

				worksheetPart.Worksheet.Append(CreateColumns(_defaultColumnWidth));

				var sheetData = new SheetData();
				sheetData.Append(GetTableTitleRow());
				sheetData.Append(GetBlankRow());
				sheetData.Append(GetSupplierRow());
				sheetData.Append(GetBlankRow());
				sheetData.Append(GetBlankRow());
				sheetData.Append(GetTableHeadersRow());

				var mergeCells = GetTitleMergeCells();

				var lastRowNum = sheetData.Elements<Row>().Count();

				for(var i = 0; i < Rows.Count; i++)
				{
					var currentRowNum = lastRowNum + i + 1;

					sheetData.Append(GetTableDataRow(i, Rows[i]));

					mergeCells.Append(new MergeCell() { Reference = new StringValue($"C{currentRowNum}:D{currentRowNum}") });
				}

				sheetData.Append(GetTableTotalRow());

				lastRowNum = sheetData.Elements<Row>().Count();

				mergeCells.Append(GetFooterMergedCells(lastRowNum));

				sheetData.Append(GetBlankRow());
				sheetData.Append(GetTotalInfoRow());
				sheetData.Append(GetTotalSumInWordsRow());
				sheetData.Append(GetBottomLineRow());
				sheetData.Append(GetBlankRow());
				sheetData.Append(GetSignatureLineRow());
				sheetData.Append(GetSignatureSmallTextRow());

				worksheetPart.Worksheet.Append(sheetData);

				worksheetPart.Worksheet.Append(mergeCells);

				worksheetPart.Worksheet.Save();

				var sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Заказы" };
				var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
				sheets.AppendChild(sheet);

				spreadsheet.WorkbookPart.Workbook.Save();
			}
		}

		private Stylesheet GetStyleSheet()
		{
			var stylesheet = new Stylesheet();

			stylesheet.Fonts = new Fonts();
			stylesheet.Fonts.AppendChild(GetDefaultFont());
			stylesheet.Fonts.AppendChild(GetTableHeadersFont());
			stylesheet.Fonts.AppendChild(GetWorksheetTitleFont());
			stylesheet.Fonts.AppendChild(GetSmallFont());
			stylesheet.Fonts.Count = 4;

			stylesheet.Fills = new Fills();
			stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } });

			stylesheet.Borders = new Borders();
			stylesheet.Borders.AppendChild(new Border());
			stylesheet.Borders.AppendChild(GetCellBorder());
			stylesheet.Borders.AppendChild(GetMediumBottomHorizontalLineCellBorder());
			stylesheet.Borders.AppendChild(GetThinBottomHorizontalLineCellBorder());
			stylesheet.Borders.Count = 4;

			var defaultCellFormat = new CellFormat { FormatId = 0, FontId = 0, BorderId = 1 };
			defaultCellFormat.Alignment = new Alignment { WrapText = true };

			var tableHeadersCellFormat = new CellFormat { FormatId = 0, FontId = 1, BorderId = 1 };
			tableHeadersCellFormat.Alignment = new Alignment { WrapText = true, Horizontal = HorizontalAlignmentValues.Center };

			var horizontalLineMediumFormat = new CellFormat { FormatId = 0, FontId = 2, BorderId = 2 };
			horizontalLineMediumFormat.Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center };

			var horizontalLineThinFormat = new CellFormat { FormatId = 0, FontId = 2, BorderId = 3 };
			horizontalLineThinFormat.Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center };

			var simpleTextFormat = new CellFormat { FormatId = 0, FontId = 0, BorderId = 0 };
			simpleTextFormat.Alignment = new Alignment { WrapText = true };

			var simpleBoldTextFormat = new CellFormat { FormatId = 0, FontId = 1, BorderId = 0 };
			simpleBoldTextFormat.Alignment = new Alignment { WrapText = true };

			var smallFontFormat = new CellFormat { FormatId = 0, FontId = 3, BorderId = 0 };
			smallFontFormat.Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center };

			stylesheet.CellStyleFormats = new CellStyleFormats();
			stylesheet.CellStyleFormats.AppendChild(new CellFormat());
			stylesheet.CellFormats = new CellFormats();
			stylesheet.CellFormats.AppendChild(new CellFormat());

			stylesheet.CellFormats.AppendChild(defaultCellFormat);
			_defaultCellFormatId = 1;

			stylesheet.CellFormats.AppendChild(tableHeadersCellFormat);
			_tableHeadersCellFormatId = 2;

			stylesheet.CellFormats.AppendChild(horizontalLineMediumFormat);
			_horizontalLineMiddleFormatId = 3;

			stylesheet.CellFormats.AppendChild(horizontalLineThinFormat);
			_horizontalLineThinFormatId = 4;

			stylesheet.CellFormats.AppendChild(simpleTextFormat);
			_simpleTextFormat = 5;

			stylesheet.CellFormats.AppendChild(simpleBoldTextFormat);
			_simpleBoldTextFormat = 6;

			stylesheet.CellFormats.AppendChild(smallFontFormat);
			_smallFontFormat = 7;

			stylesheet.CellFormats.Count = 8;

			return stylesheet;
		}

		#region Cells

		private Cell GetTableHeaderStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _tableHeadersCellFormatId
			};

			return cell;
		}

		private Cell GetSmallFontStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _smallFontFormat
			};

			return cell;
		}

		private Cell GetBottomMiddleLineStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _horizontalLineMiddleFormatId
			};

			return cell;
		}

		private Cell GetBottomThinLineStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _horizontalLineThinFormatId
			};

			return cell;
		}

		private Cell GetSimpleBoldStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _simpleBoldTextFormat
			};

			return cell;
		}

		private Cell GetSimpleStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _simpleTextFormat
			};

			return cell;
		}

		private Cell GetStringCurrencyFormatCell(decimal value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value.ToString("### ### ##0.00 ₽")),
				DataType = CellValues.String,
				StyleIndex = _defaultCellFormatId
			};

			return cell;
		}

		private Cell GetNumericCell(int value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.Number,
				StyleIndex = _defaultCellFormatId
			};

			return cell;
		}

		private Cell GetDecimalCell(decimal value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.Number,
				StyleIndex = _defaultCellFormatId
			};

			return cell;
		}

		private Cell GetStringCell(string value)
		{
			var cell = new Cell
			{
				CellValue = new CellValue(value),
				DataType = CellValues.String,
				StyleIndex = _defaultCellFormatId
			};

			return cell;
		}

		#endregion Cells

		#region MergeCells

		private MergeCells GetTitleMergeCells()
		{
			var mergeCells = new MergeCells();

			mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:I1") });
			mergeCells.Append(new MergeCell() { Reference = new StringValue("A3:A4") });
			mergeCells.Append(new MergeCell() { Reference = new StringValue("B3:I4") });
			mergeCells.Append(new MergeCell() { Reference = new StringValue("C6:D6") });
			mergeCells.Append(new MergeCell() { Reference = new StringValue("E6:F6") });

			return mergeCells;
		}

		private List<MergeCell> GetFooterMergedCells(int lastRowNum)
		{
			return new List<MergeCell>
			{
				new MergeCell() { Reference = new StringValue($"A{lastRowNum + 2}:I{lastRowNum + 2}") },
				new MergeCell() { Reference = new StringValue($"A{lastRowNum + 3}:I{lastRowNum + 3}") }
			};
		}

		#endregion MergeCells

		#region Borders

		private Border GetCellBorder()
		{
			var border = new Border();

			var leftBorder = new LeftBorder() { Style = BorderStyleValues.Thin };
			var leftBorderColor = new Color() { Indexed = (UInt32Value)64U };
			leftBorder.Append(leftBorderColor);

			var rightBorder = new RightBorder() { Style = BorderStyleValues.Thin };
			var rightBorderColor = new Color() { Indexed = (UInt32Value)64U };
			rightBorder.Append(rightBorderColor);

			var topBorder = new TopBorder() { Style = BorderStyleValues.Thin };
			var topBorderColor = new Color() { Indexed = (UInt32Value)64U };
			topBorder.Append(topBorderColor);

			var bottomBorder = new BottomBorder() { Style = BorderStyleValues.Thin };
			var bottomBorderColor = new Color() { Indexed = (UInt32Value)64U };
			bottomBorder.Append(bottomBorderColor);

			var diagonalBorder = new DiagonalBorder();

			border.Append(leftBorder);
			border.Append(rightBorder);
			border.Append(topBorder);
			border.Append(bottomBorder);
			border.Append(diagonalBorder);

			return border;
		}

		private Border GetMediumBottomHorizontalLineCellBorder()
		{
			var border = new Border();

			var leftBorder = new LeftBorder() { Style = BorderStyleValues.None };
			var leftBorderColor = new Color() { Indexed = (UInt32Value)64U };
			leftBorder.Append(leftBorderColor);

			var rightBorder = new RightBorder() { Style = BorderStyleValues.None };
			var rightBorderColor = new Color() { Indexed = (UInt32Value)64U };
			rightBorder.Append(rightBorderColor);

			var topBorder = new TopBorder() { Style = BorderStyleValues.None };
			var topBorderColor = new Color() { Indexed = (UInt32Value)64U };
			topBorder.Append(topBorderColor);

			var bottomBorder = new BottomBorder() { Style = BorderStyleValues.Medium };
			var bottomBorderColor = new Color() { Indexed = (UInt32Value)64U };
			bottomBorder.Append(bottomBorderColor);

			var diagonalBorder = new DiagonalBorder();

			border.Append(leftBorder);
			border.Append(rightBorder);
			border.Append(topBorder);
			border.Append(bottomBorder);
			border.Append(diagonalBorder);

			return border;
		}

		private Border GetThinBottomHorizontalLineCellBorder()
		{
			var border = new Border();

			var leftBorder = new LeftBorder() { Style = BorderStyleValues.None };
			var leftBorderColor = new Color() { Indexed = (UInt32Value)64U };
			leftBorder.Append(leftBorderColor);

			var rightBorder = new RightBorder() { Style = BorderStyleValues.None };
			var rightBorderColor = new Color() { Indexed = (UInt32Value)64U };
			rightBorder.Append(rightBorderColor);

			var topBorder = new TopBorder() { Style = BorderStyleValues.None };
			var topBorderColor = new Color() { Indexed = (UInt32Value)64U };
			topBorder.Append(topBorderColor);

			var bottomBorder = new BottomBorder() { Style = BorderStyleValues.Thin };
			var bottomBorderColor = new Color() { Indexed = (UInt32Value)64U };
			bottomBorder.Append(bottomBorderColor);

			var diagonalBorder = new DiagonalBorder();

			border.Append(leftBorder);
			border.Append(rightBorder);
			border.Append(topBorder);
			border.Append(bottomBorder);
			border.Append(diagonalBorder);

			return border;
		}

		#endregion Borders

		#region Fonts

		private Font GetDefaultFont()
		{
			var fontSize = new FontSize
			{
				Val = 10
			};

			var font = new Font
			{
				FontSize = fontSize
			};

			return font;
		}

		private Font GetTableHeadersFont()
		{
			var bold = new Bold();

			var fontSize = new FontSize
			{
				Val = 10
			};

			var font = new Font
			{
				Bold = bold,
				FontSize = fontSize
			};

			return font;
		}

		private Font GetWorksheetTitleFont()
		{
			var bold = new Bold();

			var fontSize = new FontSize
			{
				Val = 14
			};

			var font = new Font
			{
				Bold = bold,
				FontSize = fontSize
			};

			return font;
		}

		private Font GetSmallFont()
		{
			var fontSize = new FontSize
			{
				Val = 8
			};

			var font = new Font
			{
				FontSize = fontSize
			};

			return font;
		}

		#endregion Fonts

		#region Columns

		private Columns CreateColumns(double defaultColumnWidth)
		{
			var columns = new Columns();

			var numColumnn = CreateColumn(1, defaultColumnWidth);
			var code1cColumn = CreateColumn(2, defaultColumnWidth * 2);
			var nomenclatureColumn = CreateColumn(3, defaultColumnWidth);
			var nomenclatureEmptyColumn = CreateColumn(4, defaultColumnWidth * 5);
			var amountColumn = CreateColumn(5, defaultColumnWidth);
			var mesureColumn = CreateColumn(6, defaultColumnWidth);
			var priceColumn = CreateColumn(7, defaultColumnWidth);
			var ndsColumn = CreateColumn(8, defaultColumnWidth);
			var sumColumn = CreateColumn(9, defaultColumnWidth);

			columns.Append(numColumnn);
			columns.Append(code1cColumn);
			columns.Append(nomenclatureColumn);
			columns.Append(nomenclatureEmptyColumn);
			columns.Append(amountColumn);
			columns.Append(mesureColumn);
			columns.Append(priceColumn);
			columns.Append(ndsColumn);
			columns.Append(sumColumn);

			return columns;
		}

		private Column CreateColumn(int columnId, double columnWidth)
		{
			var column = new Column
			{
				Min = (uint)columnId,
				Max = (uint)columnId,
				CustomWidth = true,
				Width = columnWidth
			};

			return column;
		}

		#endregion Columns

		#region Rows

		private Row GetTableDataRow(int rowNum, RetailSalesReportFor1cRow node)
		{
			var row = new Row();

			row.AppendChild(GetNumericCell(rowNum + 1));
			row.AppendChild(GetStringCell(node.Code1c));
			row.AppendChild(GetStringCell(node.Nomenclature));
			row.AppendChild(GetStringCell(""));
			row.AppendChild(GetDecimalCell(node.Amount));
			row.AppendChild(GetStringCell(node.Mesure));
			row.AppendChild(GetStringCurrencyFormatCell(node.Price));
			row.AppendChild(GetStringCurrencyFormatCell(node.Nds));
			row.AppendChild(GetStringCurrencyFormatCell(node.Sum));

			return row;
		}

		private Row GetSignatureSmallTextRow()
		{
			var row = new Row();

			row.AppendChild(GetSmallFontStringCell(""));
			row.AppendChild(GetSmallFontStringCell("подпись"));
			row.AppendChild(GetSmallFontStringCell(""));
			row.AppendChild(GetSmallFontStringCell("расшифровка"));

			return row;
		}

		private Row GetSignatureLineRow()
		{
			var row = new Row();

			row.AppendChild(GetSimpleBoldStringCell("Кассир:"));
			row.AppendChild(GetBottomThinLineStringCell(""));
			row.AppendChild(GetSimpleStringCell(""));
			row.AppendChild(GetBottomThinLineStringCell(""));

			return row;
		}

		private Row GetTableHeadersRow()
		{
			var row = new Row();

			row.AppendChild(GetTableHeaderStringCell("№"));
			row.AppendChild(GetTableHeaderStringCell("Код"));
			row.AppendChild(GetTableHeaderStringCell("Номенклатура"));
			row.AppendChild(GetTableHeaderStringCell(""));
			row.AppendChild(GetTableHeaderStringCell("Количество"));
			row.AppendChild(GetTableHeaderStringCell(""));
			row.AppendChild(GetTableHeaderStringCell("Цена"));
			row.AppendChild(GetTableHeaderStringCell("Сумма НДС"));
			row.AppendChild(GetTableHeaderStringCell("Сумма"));

			return row;
		}


		private Row GetBlankRow()
		{
			var row = new Row();

			return row;
		}

		private Row GetTableTitleRow()
		{
			var row = new Row();

			row.AppendChild(GetBottomMiddleLineStringCell(Title));

			for(var i = 0; i < 8; i++)
			{
				row.AppendChild(GetBottomMiddleLineStringCell(""));
			}

			return row;
		}

		private Row GetBottomLineRow()
		{
			var row = new Row();

			for(var i = 0; i < 9; i++)
			{
				row.AppendChild(GetBottomMiddleLineStringCell(""));
			}

			return row;
		}

		private Row GetSupplierRow()
		{
			var row = new Row();

			row.AppendChild(GetSimpleStringCell("Поставщик:"));
			row.AppendChild(GetSimpleBoldStringCell(Supplier));

			return row;
		}

		private Row GetTotalInfoRow()
		{
			var row = new Row();

			row.AppendChild(GetSimpleStringCell(TotalInfo));

			return row;
		}

		private Row GetTotalSumInWordsRow()
		{
			var row = new Row();

			row.AppendChild(GetSimpleBoldStringCell(TotalSumInWords));

			return row;
		}

		private Row GetTableTotalRow()
		{
			var row = new Row();

			row.AppendChild(GetSimpleBoldStringCell(""));
			row.AppendChild(GetSimpleBoldStringCell(""));
			row.AppendChild(GetSimpleBoldStringCell(""));
			row.AppendChild(GetSimpleBoldStringCell(""));
			row.AppendChild(GetSimpleBoldStringCell(""));
			row.AppendChild(GetSimpleBoldStringCell(""));
			row.AppendChild(GetSimpleBoldStringCell("Итого"));
			row.AppendChild(GetStringCurrencyFormatCell(TotalNds));
			row.AppendChild(GetStringCurrencyFormatCell(TotalSum));

			return row;
		}

		#endregion Rows
	}
}

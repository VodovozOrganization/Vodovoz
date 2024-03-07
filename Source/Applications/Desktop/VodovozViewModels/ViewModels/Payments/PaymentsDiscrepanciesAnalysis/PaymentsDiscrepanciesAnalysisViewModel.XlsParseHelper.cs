﻿using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public static class XlsParseHelper
		{
			private const string _numberPattern = @"([0-9]{1,})";
			private const string _datePattern = @"([0-9]{2}.[0-9]{2}.[0-9]{4})";

			public static IList<IList<string>> GetRowsFromXls2(string fileName)
			{
				var xlsRows = new List<IList<string>>();

				using(var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using(var document = SpreadsheetDocument.Open(fileStream, false))
					{
						var workbookPart = document.WorkbookPart;
						var tablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
						var table = tablePart.SharedStringTable;

						var worksheetPart = workbookPart.WorksheetParts.First();
						var worksheet = worksheetPart.Worksheet;

						var cells = worksheet.Descendants<Cell>();
						var rows = worksheet.Descendants<Row>();

						var rowsCount = rows.LongCount();
						var cellsCount = cells.LongCount();

						foreach(var row in rows)
						{
							var rowData = new List<string>();

							foreach(Cell c in row.Elements<Cell>())
							{
								if((c.DataType != null) && (c.DataType == CellValues.SharedString))
								{
									int ssid = int.Parse(c.CellValue.Text);
									string str = table.ChildElements[ssid].InnerText;
									rowData.Add(str);
								}
								else if(c.CellValue != null)
								{
									rowData.Add(c.CellValue.Text);
								}
							}

							xlsRows.Add(rowData);
						}
					}
				}

				return xlsRows;
			}

			public static IList<IList<string>> GetRowsFromXls(string fileName)
			{
				if(!IsXlsxFile(fileName))
				{
					throw new ArgumentException("Попытка чтения файла, имеющего расширение отличное от \"xlsx\"");
				}

				var xlsxRowValues = new List<IList<string>>();

				using(var document = SpreadsheetDocument.Open(fileName, false))
				{
					var workbookPart = document.WorkbookPart;
					var tablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
					var sharedStringTable = tablePart.SharedStringTable;

					var worksheetPart = workbookPart.WorksheetParts.First();
					var worksheet = worksheetPart.Worksheet;

					var rows = worksheet.Descendants<Row>();

					xlsxRowValues = GetRowsCellsValues(rows, sharedStringTable).ToList();
				}

				return xlsxRowValues;
			}

			private static bool IsXlsxFile(string fileName)
			{
				var extension = Path.GetExtension(fileName).ToLower();

				return extension == ".xlsx";
			}

			private static IList<IList<string>> GetRowsCellsValues(IEnumerable<Row> rows, SharedStringTable sharedStringTable)
			{
				var rowsValues = new List<IList<string>>();

				foreach(var row in rows)
				{
					var rowData = GetRowCellsValues(row, sharedStringTable);

					rowsValues.Add(rowData);
				}

				return rowsValues;
			}

			private static IList<string> GetRowCellsValues(Row row, SharedStringTable sharedStringTable)
			{
				var rowData = new List<string>();

				foreach(Cell cell in row.Elements<Cell>())
				{
					var cellValue = GetCellValue(cell, sharedStringTable);

					rowData.Add(cellValue);
				}

				return rowData;
			}

			private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
			{
				string cellValue;

				if((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
				{
					int ssid = int.Parse(cell.CellValue.Text);
					cellValue = sharedStringTable.ChildElements[ssid].InnerText;
				}
				else
				{
					cellValue = cell.CellValue?.Text ?? string.Empty;
				}

				return cellValue;
			}

			public static int ParseNumberFromString(string str)
			{
				var matches = Regex.Matches(str, _numberPattern);
				return int.Parse(matches[0].Value);
			}

			public static DateTime ParseDateFromString(string str)
			{
				var matches = Regex.Matches(str, _datePattern);
				return DateTime.Parse(matches[0].Value);
			}

			public static string ParseClientInnFromString(string str)
			{
				var matches = Regex.Matches(str, _numberPattern);
				return matches[0].Value;
			}
		}
	}
}

using DocumentFormat.OpenXml.Packaging;
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
			private const string _floatingPointNumberPattern = @"^-?\d*\,{0,1}\d+$";
			private const string _datePattern = @"([0-9]{2}.[0-9]{2}.[0-9]{4})";

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

			public static int? ParseNumberFromString(string str)
			{
				if(str is null)
				{
					return null;
				}

				var matches = Regex.Matches(str, _numberPattern);

				if(matches.Count > 0)
				{
					return int.Parse(matches[0].Value);
				}

				return null;
			}

			public static decimal? ParseFloatingPointNumberFromString(string str)
			{
				if(str is null)
				{
					return null;
				}

				var matches = Regex.Matches(str, _floatingPointNumberPattern);

				if(matches.Count > 0)
				{
					return decimal.Parse(matches[0].Value);
				}

				return null;
			}

			public static DateTime? ParseDateFromString(string str)
			{
				if(str is null)
				{
					return null;
				}

				var matches = Regex.Matches(str, _datePattern);

				if(matches.Count > 0)
				{
					return DateTime.Parse(matches[0].Value);
				}

				return null;
			}

			public static string ParseClientInnFromString(string str)
			{
				if(str is null)
				{
					return null;
				}

				var matches = Regex.Matches(str, _numberPattern);

				if(matches.Count > 0)
				{
					for(int i = 0; i < matches.Count; i++)
					{
						var matchValue = matches[i].Value;

						if(matchValue.Length == 10 || matchValue.Length == 12)
						{
							return matchValue;
						}
					}
				}

				return null;
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
		}
	}
}

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
			private const string _datePattern = @"([0-9]{2}.[0-9]{2}.[0-9]{4})";

			public static IList<IList<string>> GetRowsFromXls(string fileName)
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

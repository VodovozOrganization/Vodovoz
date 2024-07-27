using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader;

namespace Vodovoz.Application.BankStatements
{
	public class BankStatementParser
	{
		public IEnumerable<IEnumerable<string>> TryParse(string filePath, BankStatementFileExtension fileExtension)
		{
			IEnumerable<IEnumerable<string>> parsedData = null;

			switch(fileExtension)
			{
				case BankStatementFileExtension.xls:
					parsedData = ParseXls(filePath);
					break;
				case BankStatementFileExtension.xlsx:
					parsedData = ParseXlsx(filePath);
					break;
				case BankStatementFileExtension.xml:
					parsedData = ParseXml(filePath);
					break;
			}
			
			return parsedData;
		}
		
		private IEnumerable<IEnumerable<string>> ParseXlsx(string filePath)
		{
			var stringRows = new List<IEnumerable<string>>();
			
			using(var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
			{
				using(var reader = ExcelReaderFactory.CreateReader(stream))
				{
					var dataset = reader.AsDataSet();

					if(dataset.Tables.Count == 0)
					{
						return stringRows;
					}

					var rows = dataset.Tables[0].Rows;

					for(var i = 0; i < rows.Count; i++)
					{
						stringRows.Add(rows[i].ItemArray
							.Select(x => x.ToString())
							.Where(x => !string.IsNullOrWhiteSpace(x)));
					}
				}
			}

			return stringRows;
			/*var xlsxRowValues = new List<IList<string>>();

			using(var document = SpreadsheetDocument.Open(filePath, false))
			{
				var workbookPart = document.WorkbookPart;
				var tablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
				var sharedStringTable = tablePart.SharedStringTable;

				var worksheetPart = workbookPart.WorksheetParts.First();
				var worksheet = worksheetPart.Worksheet;

				var rows = worksheet.Descendants<Row>();

				xlsxRowValues = GetRowsCellsValues(rows, sharedStringTable).ToList();
			}

			return xlsxRowValues;*/
		}

		private IEnumerable<IEnumerable<string>> ParseXls(string filePath)
		{
			var stringRows = new List<IEnumerable<string>>();
			
			using(var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
			{
				using(var reader = ExcelReaderFactory.CreateReader(stream))
				{
					var dataset = reader.AsDataSet();

					if(dataset.Tables.Count == 0)
					{
						return stringRows;
					}

					var rows = dataset.Tables[0].Rows;

					for(var i = 0; i < rows.Count; i++)
					{
						stringRows.Add(rows[i].ItemArray
							.Select(x => x.ToString())
							.Where(x => !string.IsNullOrWhiteSpace(x)));
					}
				}
			}

			return stringRows;
		}
		
		private IEnumerable<IEnumerable<string>> ParseXml(string filePath)
		{
			var serializer = new XmlSerializer(typeof(VodovozInfrastructure.BankStatements.Workbook));
			VodovozInfrastructure.BankStatements.Workbook workbook;

			using(var reader = new StreamReader(filePath))
			{
				workbook = (VodovozInfrastructure.BankStatements.Workbook)serializer.Deserialize(reader);
			}

			IEnumerable<string> result = Array.Empty<string>();
			
			if(workbook.Worksheet.Any())
			{
				result =
					workbook
						.Worksheet.First()
						.Table.Row.SelectMany(x => x.Cell)
						.Where(x => x.Data != null && !string.IsNullOrWhiteSpace(x.Data.Value))
						.Select(x => x.Data.Value);
			}

			var list = new List<IEnumerable<string>>
			{
				result
			};

			return list;
		}
		
		private IList<IList<string>> GetRowsCellsValues(IEnumerable<Row> rows, SharedStringTable sharedStringTable)
		{
			return rows
				.Select(row => GetRowCellsValues(row, sharedStringTable))
				.Where(rowData => rowData.Any())
				.ToList();
		}
		
		private IList<string> GetRowCellsValues(Row row, SharedStringTable sharedStringTable)
		{
			return row.Elements<Cell>()
				.Select(cell => GetCellValue(cell, sharedStringTable))
				.Where(cellValue => !string.IsNullOrWhiteSpace(cellValue))
				.ToList();
		}
		
		private string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
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

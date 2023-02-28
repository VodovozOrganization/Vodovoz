using ClosedXML.Excel;
using QS.Project.Services.FileDialog;
using RevenueService.Client.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using VodovozInfrastructure.Attributes;

namespace Vodovoz.ViewModels.ViewModels.Reports.Counterparty
{
	public class CounterpartyRevenueServiceReport
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IList<CounterpartyRevenueServiceDto> _rows;

		public CounterpartyRevenueServiceReport(IList<CounterpartyRevenueServiceDto> rows, IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_rows = rows ?? throw new ArgumentNullException(nameof(rows)); ;
		}

		public void Export()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{"ФНС"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				SaveReport(result.Path);
			}
		}

		private void SaveReport(string path)
		{
			var propertyArray = typeof(CounterpartyRevenueServiceDto).GetProperties()
				.Where(x => !x.GetCustomAttributes<ReportExportIgnoreAttribute>(true).Any())
				.ToArray();

			using(var workbook = new XLWorkbook())
			{
				foreach(var row in _rows)
				{
					var sheetName = $"{_rows.IndexOf(row) + 1}. {row.Name.Substring(0, Math.Min(row.Name.Length, 20))}";
					var worksheet = workbook.Worksheets.Add(sheetName);

					worksheet.Column(1).Width = 50;
					worksheet.Column(2).Width = 50;

					worksheet.Cell(1, 1).Value = "Параметр";
					worksheet.Cell(1, 2).Value = "Значение";

					for(int i = 0; i < propertyArray.Length; i++)
					{
						var displayAttr = propertyArray[i].GetCustomAttributes<DisplayAttribute>(true)
							.SingleOrDefault()
							?.Name;

						worksheet.Cell(i + 1, 1).Value = displayAttr ?? propertyArray[i].Name;

						var propValue = propertyArray[i].GetValue(row);

						string resVaue;

						if(propValue is Array arrayValue)
						{
							var valueList = new List<string>();

							foreach(var value in arrayValue)
							{
								valueList.Add(value.ToString());
							}

							resVaue = string.Join("; ", valueList);
						}
						else
						{
							resVaue = propValue?.ToString();
						}

						worksheet.Cell(i + 1, 2).Value = resVaue;
					}

					for(int c = 1; c <= 2; c++)
					{
						for(int r = 1; r <= _rows.Count + 1; r++)
						{
							worksheet.Cell(r, c).Style.Alignment.WrapText = true;
						}
					}
				}

				workbook.SaveAs(path);
			}
		}
	}
}

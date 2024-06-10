using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using QS.Project.Services.FileDialog;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics
{
	public class CompletedDriversWarehousesEventsJournalReport
	{
		private readonly IFileDialogService _fileDialogService;

		public CompletedDriversWarehousesEventsJournalReport(
			IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
		}
		
		public void Export(IEnumerable<CompletedDriversWarehousesEventsJournalNode> rows)
		{
			const string journalName = "Журнал завершенных событий";
			const string extension = ".xlsx";
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = extension,
				FileName = $"{journalName} {DateTime.Now:yyyy-MM-dd-HH-mm}{extension}"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			
			if(result.Successful)
			{
				using(var wb = new XLWorkbook())
				{
					Generate(wb, journalName, rows);
					wb.SaveAs(result.Path);
				}
			}
		}

		private void Generate(
			IXLWorkbook wb,
			string journalName,
			IEnumerable<CompletedDriversWarehousesEventsJournalNode> rows)
		{
			var ws = wb.Worksheets.Add(journalName);

			var colNames = new[]
			{
				CompletedDriversWarehousesEventsJournalNode.IdColumn,
				CompletedDriversWarehousesEventsJournalNode.EventNameColumn,
				CompletedDriversWarehousesEventsJournalNode.EventTypeColumn,
				CompletedDriversWarehousesEventsJournalNode.DocumentTypeColumn,
				CompletedDriversWarehousesEventsJournalNode.DocumentNumberColumn,
				CompletedDriversWarehousesEventsJournalNode.EmployeeColumn,
				CompletedDriversWarehousesEventsJournalNode.CarColumn,
				CompletedDriversWarehousesEventsJournalNode.CompletedDateColumn,
				CompletedDriversWarehousesEventsJournalNode.DistanceColumn
			};

			var row = 1;
			var col = 1;
			foreach(var name in colNames)
			{
				ws.Cell(row, col).Value = name;
				col++;
			}

			foreach(var newRow in rows)
			{
				row++;
				col = 1;
				ws.Cell(row, col).Value = newRow.Id;
				ws.Cell(row, ++col).Value = newRow.EventName;
				ws.Cell(row, ++col).Value = newRow.EventType.GetEnumDisplayName();
				ws.Cell(row, ++col).Value = newRow.DocumentType?.GetEnumDisplayName();
				ws.Cell(row, ++col).Value = newRow.DocumentNumber;
				ws.Cell(row, ++col).Value = newRow.EmployeeName;
				ws.Cell(row, ++col).Value = newRow.Car;
				ws.Cell(row, ++col).Value = newRow.CompletedDate;
				ws.Cell(row, ++col).Value = newRow.DistanceMetersFromScanningLocation;
			}
		}
	}
}

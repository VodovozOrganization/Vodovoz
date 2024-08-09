using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Gamma.Utilities;
using QS.Project.Services.FileDialog;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;

namespace Vodovoz.ViewModels.ViewModels.Reports.Orders
{
	public class OrdersRatingsJournalReport
	{
		private readonly IFileDialogService _fileDialogService;

		public OrdersRatingsJournalReport(
			IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
		}
		
		public void Export(IEnumerable<OrdersRatingsJournalNode> rows)
		{
			const string journalName = "Журнал оценок заказов";
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
			IEnumerable<OrdersRatingsJournalNode> rows)
		{
			var ws = wb.Worksheets.Add(journalName);

			var colNames = new[]
			{
				OrdersRatingsJournalNode.IdColumn,
				OrdersRatingsJournalNode.OnlineOrderIdColumn,
				OrdersRatingsJournalNode.OrderIdColumn,
				OrdersRatingsJournalNode.CreatedColumn,
				OrdersRatingsJournalNode.StatusColumn,
				OrdersRatingsJournalNode.RatingColumn,
				OrdersRatingsJournalNode.ReasonsColumn,
				OrdersRatingsJournalNode.CommentColumn,
				OrdersRatingsJournalNode.SourceColumn
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
				ws.Cell(row, ++col).Value = newRow.OnlineOrderId;
				ws.Cell(row, ++col).Value = newRow.OrderId;
				ws.Cell(row, ++col).Value = newRow.OrderRatingCreated;
				ws.Cell(row, ++col).Value = newRow.OrderRatingStatus.GetEnumTitle();
				ws.Cell(row, ++col).Value = newRow.Rating;
				ws.Cell(row, ++col).Value = newRow.OrderRatingReasons;
				ws.Cell(row, ++col).Value = newRow.OrderRatingComment;
				ws.Cell(row, ++col).Value = newRow.OrderRatingSource.GetEnumTitle();
			}
		}
	}
}

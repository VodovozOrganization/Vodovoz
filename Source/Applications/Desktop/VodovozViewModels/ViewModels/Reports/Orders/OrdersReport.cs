using QS.DomainModel.Entity;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using Vodovoz.JournalNodes;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.ViewModels.Reports.Orders
{
	[Appellative(Nominative = "Отчет по заказам")]
	public class OrdersReport
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		public OrdersReport(
			DateTime createDateFrom, 
			DateTime createDateTo, 
			IEnumerable<OrderJournalNode> rows,
			IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_report = new Report(rows);

			CreateDateFrom = createDateFrom;
			CreateDateTo = createDateTo;
			ReportCreatedAt = DateTime.Now;
		}

		public string Title => typeof(OrdersReport).GetClassUserFriendlyName().Nominative;

		public DateTime CreateDateFrom { get; }

		public DateTime CreateDateTo { get; }

		public DateTime ReportCreatedAt { get; }

		public void Export()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{Title} {ReportCreatedAt:yyyy-MM-dd-HH-mm}.xlsx";

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				SaveReport(result.Path);
			}
		}

		private void SaveReport(string path)
		{

		}

		private class Report
		{
			public Report(IEnumerable<OrderJournalNode> rows)
			{
				Rows = rows;
			}

			IEnumerable<OrderJournalNode> Rows { get; }
		}
	}
}

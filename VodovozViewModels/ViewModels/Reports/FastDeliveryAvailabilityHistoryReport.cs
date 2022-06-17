using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using ClosedXML.Report;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class FastDeliveryAvailabilityHistoryReport
	{
		private const string _templatePath = @".\Reports\Orders\FastDeliveryAvailabilityHistoryReport.xlsx";
		private readonly IList<FastDeliveryAvailabilityHistoryJournalNode> _rows;
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		public FastDeliveryAvailabilityHistoryReport(IList<FastDeliveryAvailabilityHistoryJournalNode> rows, IFileDialogService fileDialogService)
		{
			_rows = rows ?? throw new ArgumentNullException(nameof(rows));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_report = new Report(rows);
		}

		public void Export()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{"Доставка за час"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				SaveReport(result.Path);
			}
		}

		private void SaveReport(string path)
		{
			var template = new XLTemplate(_templatePath);
			template.AddVariable(_report);
			template.Generate();
			template.SaveAs(path);
		}

		private class Report
		{
			public Report(IList<FastDeliveryAvailabilityHistoryJournalNode> rows)
			{
				Rows = rows;
			}

			public IList<FastDeliveryAvailabilityHistoryJournalNode> Rows { get; set; }
		}
	}
}

using ClosedXML.Report;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels.Reports.DebtorsJournalReport
{
	public class DebtorsJournalReport
	{
		private const string _templatePath = @".\Reports\Client\DebtorsJournalReport.xlsx";
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		public DebtorsJournalReport(IList<DebtorJournalNode> rows, IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_report = new Report(rows);
		}

		public void Export()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{"Журнал задолженностей"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

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
			public Report(IList<DebtorJournalNode> rows)
			{
				Rows = rows;
			}

			public IList<DebtorJournalNode> Rows { get; }
		}
	}
}

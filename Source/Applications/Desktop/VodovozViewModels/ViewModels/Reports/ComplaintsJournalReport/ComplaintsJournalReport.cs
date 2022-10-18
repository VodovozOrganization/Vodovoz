using ClosedXML.Report;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using Vodovoz.Journals.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels.Reports.ComplaintsJournalReport
{
	public class ComplaintJournalReport
	{
		private const string _templatePath = @".\Reports\Complaints\ComplaintsJournalReport.xlsx";
		private readonly IList<ComplaintJournalNode> _rows;
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		public ComplaintJournalReport(IList<ComplaintJournalNode> rows, IFileDialogService fileDialogService)
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
			dialogSettings.FileName = $"{"Журнал рекламаций"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

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
			public Report(IList<ComplaintJournalNode> rows)
			{
				Rows = rows;
			}

			public IList<ComplaintJournalNode> Rows { get; }
		}
	}
}

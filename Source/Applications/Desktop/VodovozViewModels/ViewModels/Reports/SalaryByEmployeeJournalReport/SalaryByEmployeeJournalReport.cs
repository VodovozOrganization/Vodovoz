using ClosedXML.Report;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;

namespace Vodovoz.ViewModels.ViewModels.Reports.SalaryByEmployeeJournalReport
{
	public class SalaryByEmployeeJournalReport
	{
		private const string _templatePath = @".\Reports\Cash\SalaryByEmployeeJournalReport.xlsx";
		private readonly IList<EmployeeWithLastWorkingDayJournalNode> _rows;
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		public SalaryByEmployeeJournalReport(IList<EmployeeWithLastWorkingDayJournalNode> rows, IFileDialogService fileDialogService)
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
			dialogSettings.FileName = $"{"Журнал выдач ЗП"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

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
			public Report(IList<EmployeeWithLastWorkingDayJournalNode> rows)
			{
				Rows = rows;
			}

			public IList<EmployeeWithLastWorkingDayJournalNode> Rows { get; }
		}
	}
}

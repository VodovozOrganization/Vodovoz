using ClosedXML.Report;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels.Reports.ComplaintsJournalReport
{
	public class ComplaintClassificationSummaryReport
	{
		private const string _templatePath = @".\Reports\Complaints\ComplaintClassificationSummaryReport.xlsx";
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		public ComplaintClassificationSummaryReport(IList<ComplaintJournalNode> journalNodes, ComplaintFilterViewModel complaintFilterViewModel, IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			if(complaintFilterViewModel == null)
			{
				throw new ArgumentNullException(nameof(complaintFilterViewModel));
			}

			var title = GenerateTitle(complaintFilterViewModel);

			_report = new Report(journalNodes, title);
		}

		private string GenerateTitle(ComplaintFilterViewModel filter)
		{
			var generateDate = $"Время выгрузки: {DateTime.Now}";
			StringBuilder title = new StringBuilder(generateDate);

			if(filter.StartDate.HasValue)
			{
				title.Append($", начальная дата: {filter.StartDate:d}");
			}

			title.Append($", конечная дата: {filter.EndDate:d}");


			if(filter.Subdivision != null)
			{
				title.Append($", в работе у: {filter.Subdivision.ShortName}");
			}

			title.Append($", тип даты: {filter.FilterDateType.GetEnumTitle()}");


			if(filter.ComplaintType != null)
			{
				title.Append($", тип рекламации: {filter.ComplaintType.GetEnumTitle()}");
			}

			if(filter.ComplaintStatus != null)
			{
				title.Append($", статус рекламации: {filter.ComplaintStatus.GetEnumTitle()}");
			}

			if(filter.Employee != null)
			{
				title.Append($", сотрудник: {filter.Employee.GetPersonNameWithInitials()}");
			}

			if(filter.Counterparty != null)
			{
				title.Append($", клиент: {filter.Counterparty.Name}");
			}

			if(filter.CurrentUserSubdivision != null && filter.ComplaintDiscussionStatus != null)
			{
				title.Append($", статус в отделе {filter.CurrentUserSubdivision.Name}: {filter.ComplaintDiscussionStatus.GetEnumTitle()}");
			}

			if(filter.GuiltyItemVM?.Entity?.Responsible != null)
			{
				title.Append($", ответственный: {filter.GuiltyItemVM.Entity.Responsible.Name} ");
				if(filter.GuiltyItemVM.Entity.Responsible.IsEmployeeResponsible && filter.GuiltyItemVM.Entity.Employee != null)
				{
					title.Append(filter.GuiltyItemVM.Entity.Employee.GetPersonNameWithInitials());
				}

				if(filter.GuiltyItemVM.Entity.Responsible.IsSubdivisionResponsible && filter.GuiltyItemVM.Entity.Subdivision != null)
				{
					title.Append(filter.GuiltyItemVM.Entity.Subdivision.Name);
				}
			}

			if(filter.ComplainDetalization != null)
			{
				title.Append($", детализация: {filter.ComplainDetalization.GetFullName}");

			}

			if(filter.ComplaintKind != null)
			{
				title.Append($", вид: {filter.ComplaintKind.GetFullName}");
			}

			if(filter.ComplaintObject != null)
			{
				title.Append($", объект: {filter.ComplaintObject.Name}");
			}

			return title.ToString();
		}

		public void Export()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{"Сводка по классификации рекламаций"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

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
			public Report(IList<ComplaintJournalNode> journalNodes, string title)
			{
				Rows = GenerateRows(journalNodes);
				Title = title;
			}

			private IEnumerable<ReportNode> GenerateRows(IList<ComplaintJournalNode> journalNodes)
			{
				var reportRows = journalNodes.GroupBy(jn => new { jn.ComplaintObjectString, jn.ComplaintKindString, jn.ComplaintDetalizationString })
					.Select(gr =>
						new ReportNode
						{
							ComplaintObject = gr.Key.ComplaintObjectString,
							ComplaintKind = gr.Key.ComplaintKindString,
							ComplaintDetalization = gr.Key.ComplaintDetalizationString,
							Amount = gr.Count()
						})
					.OrderByDescending(rn => rn.Amount);

				return reportRows;
			}

			public class ReportNode
			{
				public string ComplaintObject { get; set; }
				public string ComplaintKind { get; set; }
				public string ComplaintDetalization { get; set; }
				public int Amount { get; set; }

			}

			public IEnumerable<ReportNode> Rows { get; }
			public string Title { get; }
		}
	}
}

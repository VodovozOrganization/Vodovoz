using Gamma.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels.Reports.ComplaintsJournalReport
{
	public partial class ComplaintClassificationSummaryReport
	{
		public const string TemplatePath = @".\Reports\Complaints\ComplaintClassificationSummaryReport.xlsx";

		private ComplaintClassificationSummaryReport(IList<ComplaintJournalNode> journalNodes, string details)
		{
			Details = details;

			Title = $"Детализация рекламаций за {DateTime.Today:dd.MM.yyyy}";

			Rows = GenerateRows(journalNodes);
		}

		public string FileName = $"{"Сводка по классификации рекламаций"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

		public IEnumerable<ReportNode> Rows { get; }

		public string Title { get; }

		public string Details { get; }

		private static string GenerateDetails(ComplaintFilterViewModel filter)
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

		private IEnumerable<ReportNode> GenerateRows(IList<ComplaintJournalNode> journalNodes)
		{
			var reportRows = journalNodes.GroupBy(jn => new { jn.ComplaintObjectString, jn.ComplaintKindString, jn.ComplaintDetalizationString })
				.Select(gr =>
					new ReportNode
					{
						Guilties = GenerateGuilties(gr.Select(item => item.Guilties)),
						ComplaintObject = gr.Key.ComplaintObjectString,
						ComplaintKind = gr.Key.ComplaintKindString,
						ComplaintDetalization = gr.Key.ComplaintDetalizationString,
						Amount = gr.Count()
					})
				.OrderByDescending(rn => rn.Amount)
				.ToList();

			for(int i = 0; i < reportRows.Count; i++)
			{
				reportRows[i].Number = i + 1;
			}

			return reportRows;
		}

		private string GenerateGuilties(IEnumerable<string> allGuilties)
		{
			var guilties = new List<string>();

			foreach(var guilty in allGuilties)
			{
				if(guilty is null)
				{
					continue;
				}

				var splittedGuilties = guilty.Split('\n');

				foreach(var subGuilty in splittedGuilties)
				{
					if(!guilties.Contains(subGuilty))
					{
						guilties.Add(subGuilty);
					}
				}
			}

			return string.Join("/", guilties);
		}

		public static ComplaintClassificationSummaryReport Generate(
			IList<ComplaintJournalNode> journalNodes,
			ComplaintFilterViewModel complaintFilterViewModel)
		{
			return new ComplaintClassificationSummaryReport(journalNodes, GenerateDetails(complaintFilterViewModel));
		}
	}
}

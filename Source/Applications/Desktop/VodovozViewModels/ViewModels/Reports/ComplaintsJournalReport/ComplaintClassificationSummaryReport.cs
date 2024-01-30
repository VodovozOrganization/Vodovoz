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

		private ComplaintClassificationSummaryReport(IList<ComplaintJournalNode> journalNodes, string details, DateTime? startDate, DateTime endDate)
		{
			Details = details;

			if(!startDate.HasValue || startDate?.Date == endDate.Date)
			{
				Title = $"Детализация рекламаций за {endDate:dd.MM.yyyy}";
			}
			else
			{
				Title = $"Детализация рекламаций с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
			}

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
			var reportRows = journalNodes.GroupBy(jn => new { jn.Guilties, jn.ComplaintObjectString, jn.ComplaintKindString, jn.ComplaintDetalizationString })
				.Select(gr =>
					new ReportNode
					{
						Guilties = gr.Key.Guilties?.Replace('\n', '/') ?? "",
						ComplaintObject = gr.Key.ComplaintObjectString,
						ComplaintKind = gr.Key.ComplaintKindString,
						ComplaintDetalization = gr.Key.ComplaintDetalizationString,
						Amount = gr.Count()
					})
				.OrderBy(row => row.Guilties)
				.ThenBy(row => row.ComplaintObject)
				.ThenBy(row => row.ComplaintKind)
				.ToList();

			for(int i = 0; i < reportRows.Count; i++)
			{
				reportRows[i].Number = i + 1;
			}

			return reportRows;
		}
		
		public static ComplaintClassificationSummaryReport Generate(
			IList<ComplaintJournalNode> journalNodes,
			ComplaintFilterViewModel complaintFilterViewModel)
		{
			if(!complaintFilterViewModel.EndDate.HasValue)
			{
				throw new ArgumentException("Не выбран интервал");
			}

			return new ComplaintClassificationSummaryReport(
				journalNodes,
				GenerateDetails(complaintFilterViewModel),
				complaintFilterViewModel.StartDate,
				complaintFilterViewModel.EndDate.Value);
		}
	}
}

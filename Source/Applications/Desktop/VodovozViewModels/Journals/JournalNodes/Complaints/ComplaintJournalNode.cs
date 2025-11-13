using System;
using Gamma.Utilities;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Journals.JournalNodes
{
	public class ComplaintJournalNode : JournalEntityNodeBase
	{
		private string _complaintKindString;
		private string _complaintDetalizationString;

		protected ComplaintJournalNode() : base(typeof(Complaint))
		{
		}

		public override string Title => $"{TypeString} №{Id} от {DateString}";

		public ComplaintType Type { get; set; }

		public string TypeString
		{
			get
			{
				switch(Type)
				{
					case ComplaintType.Inner:
						return "ВН";
					case ComplaintType.Client:
						return "КЛ";
				}
				return Type.GetEnumTitle();
			}
		}

		public int SequenceNumber { get; set; }

		public DateTime Date { get; set; }
		public string DateString => Date.ToString("dd.MM.yy");
		public string TimeString => Date.ToString("t");

		public ComplaintStatuses Status { get; set; }
		public bool IsNeedWork { get; set; }
		public string StatusString => Status.GetEnumTitle();

		public string WorkInSubdivision { get; set; }

		public DateTime LastPlannedCompletionDate { get; set; }
		public string PlannedCompletionDate { get; set; }

		public string ClientNameWithAddress { get; set; }

		public string Guilties { get; set; }
		public string Driver { get; set; }

		public string ComplaintText { get; set; }

		public string Author { get; set; }

		public string Fines { get; set; }

		public string ResultText { get; set; }
		public string ResultCommentsAuthors { get; set; }

		public DateTime? ActualCompletionDate { get; set; }
		public string ActualCompletionDateString => ActualCompletionDate.HasValue ? ActualCompletionDate.Value.ToString("dd.MM.yy") : "-";

		public string DaysInWork
		{
			get
			{
				if(ActualCompletionDate.HasValue)
				{
					return (ActualCompletionDate.Value - Date).TotalDays.ToString("F0");
				}
				return "-";
			}
		}

		public bool ComplaintKindIsArchive { get; set; }

		public string ComplaintKindString
		{
			get => ComplaintKindIsArchive ? string.Format("(Архив) {0}", _complaintKindString) : _complaintKindString;
			set => _complaintKindString = value;
		}

		public bool ComplaintDetalizationIsArchive { get; set; }

		public string ComplaintDetalizationString
		{
			get => ComplaintDetalizationIsArchive ? string.Format("(Архив) {0}", _complaintDetalizationString) : _complaintDetalizationString;
			set => _complaintDetalizationString = value;
		}

		public string ComplaintObjectString { get; set; }

		public string ResultOfCounterparty { get; set; }
		public string ResultOfEmployees { get; set; }
		public string ArrangementText { get; set; }
	}

	public class ComplaintWithDepartmentsReactionJournalNode : ComplaintJournalNode
	{
		public DateTime? DepartmentConnectionTime { get; set; }
		public DateTime? DepartmentFirstCommentTime { get; set; }

		public string DepartmentConnectionTimeString =>
			DepartmentConnectionTime.HasValue
			? $"{DepartmentConnectionTime.Value.ToString("dd.MM.yy")} {DepartmentConnectionTime.Value.ToString("t")}"
			: "-";

		public string DepartmentFirstCommentTimeString =>
			DepartmentFirstCommentTime.HasValue
			? $"{DepartmentFirstCommentTime.Value.ToString("dd.MM.yy")} {DepartmentFirstCommentTime.Value.ToString("t")}"
			: "-";

		public string DepartmentReactionTimeString
		{
			get
			{
				if(DepartmentConnectionTime.HasValue && DepartmentFirstCommentTime.HasValue)
				{
					var reactionTime = DepartmentFirstCommentTime.Value - DepartmentConnectionTime.Value;

					if(reactionTime.Minutes % 10 != (DepartmentFirstCommentTime.Value.Minute - DepartmentConnectionTime.Value.Minute) % 10)
					{
						reactionTime = reactionTime + TimeSpan.FromMinutes(1);
					}

					string daysCountStr = $"{reactionTime.Days.ToString("F0")}д";
					string hoursCountStr = $"{reactionTime.Hours.ToString("F0")}ч";
					string minutesCountStr = $"{reactionTime.Minutes.ToString("F0")}мин";

					return $"{daysCountStr}:{hoursCountStr}:{minutesCountStr}";
				}

				return "-";
			}
		}
	}
}

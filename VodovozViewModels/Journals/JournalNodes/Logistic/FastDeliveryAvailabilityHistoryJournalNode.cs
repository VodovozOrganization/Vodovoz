using QS.Project.Journal;
using System;
using Vodovoz.Domain.Logistic.FastDelivery;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class FastDeliveryAvailabilityHistoryJournalNode : JournalEntityNodeBase<FastDeliveryAvailabilityHistory>
	{
		public DateTime VerificationDate { get; set; }
		public string Author { get; set; }
		public int Order { get; set; }
		public string Counterparty { get; set; }
		public string Address { get; set; }
		public string District { get; set; }
		public bool IsValid { get; set; }
		public string Logistician { get; set; }
		public string LogisticianComment { get; set; }
		public DateTime LogisticianCommentVersion { get; set; }
		public int RowNum { get; set; }

		public string IsValidString => IsValid ? "Да" : "";

		public string VerificationDateString => VerificationDate.ToString("dd.MM.yy HH:mm:ss");

		public string AuthorString => string.IsNullOrEmpty(Author) ? "Сайт" : Author;

		public string LogisticianCommentVersionString => LogisticianCommentVersion > DateTime.MinValue
			? LogisticianCommentVersion.ToString("dd.MM.yy HH:mm:ss")
			: "";
		public string LogisticianReactionTime
		{
			get
			{
				var reactionTimeString = "";
				if(LogisticianCommentVersion > DateTime.MinValue)
				{
					var ts = LogisticianCommentVersion - VerificationDate;
					reactionTimeString += $"{((int)ts.TotalHours):00}:{ts.Minutes:00}";
				}

				return reactionTimeString;
			}
		}

	}
}

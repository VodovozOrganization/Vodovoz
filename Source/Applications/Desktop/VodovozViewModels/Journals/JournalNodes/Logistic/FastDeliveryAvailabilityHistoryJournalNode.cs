using QS.Project.Journal;
using System;
using QS.Utilities.Text;
using Vodovoz.Domain.Logistic.FastDelivery;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class FastDeliveryAvailabilityHistoryJournalNode : JournalEntityNodeBase<FastDeliveryAvailabilityHistory>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public int SequenceNumber { get; set; }
		public DateTime VerificationDate { get; set; }
		public string AuthorName { get; set; }
		public string AuthorLastName { get; set; }
		public string AuthorPatronymic { get; set; }
		public int Order { get; set; }
		public string Counterparty { get; set; }
		public string AddressFromDeliveryPoint { get; set; }
		public string AddressWithoutDeliveryPoint { get; set; }
		public string District { get; set; }
		public bool IsValid { get; set; }
		public bool IsNomenclatureNotInStockSubquery { get; set; }
		public string LogisticianName { get; set; }
		public string LogisticianLastName { get; set; }
		public string LogisticianPatronymic { get; set; }
		public string LogisticianComment { get; set; }
		public DateTime LogisticianCommentVersion { get; set; }
		public string IsValidString => IsValid ? "Да" : "Нет";
		public string IsNomenclatureNotInStockSubqueryString => IsNomenclatureNotInStockSubquery ? "Да" : "Нет";
		public string VerificationDateString => VerificationDate.ToString("dd.MM.yy HH:mm:ss");

		public string AuthorString => string.IsNullOrEmpty(AuthornNameWithInitials) ? "Сайт" : AuthornNameWithInitials;

		public string AuthornNameWithInitials => PersonHelper.PersonNameWithInitials(AuthorLastName, AuthorName, AuthorPatronymic);

		public string LogisticianNameWithInitials => PersonHelper.PersonNameWithInitials(LogisticianLastName, LogisticianName, LogisticianPatronymic);

		public string AddressString => AddressFromDeliveryPoint ?? AddressWithoutDeliveryPoint;

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
					reactionTimeString += $"{ts.TotalHours:00}:{ts.Minutes:00}";
				}

				return reactionTimeString;
			}
		}
	}
}

using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "обсуждения",
		Nominative = "обсуждение"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintDiscussion : PropertyChangedBase, IDomainObject
	{
		public int Id { get; set; }

		private Complaint complaint;
		[Display(Name = "Жалоба")]
		public virtual Complaint Complaint {
			get => complaint;
			set => SetField(ref complaint, value, () => Complaint);
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value, () => Subdivision);
		}

		private DateTime plannedCompletionDate;
		[Display(Name = "Предполагаемая дата завершения")]
		public virtual DateTime PlannedCompletionDate {
			get => plannedCompletionDate;
			set => SetField(ref plannedCompletionDate, value, () => PlannedCompletionDate);
		}

		private ComplaintStatuses status;
		[Display(Name = "Статус")]
		public virtual ComplaintStatuses Status {
			get => status;
			set => SetField(ref status, value, () => Status);
		}

		public ComplaintDiscussion()
		{
		}

	}
}

using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "комментарии к обсуждению жалобы",
		Nominative = "комментарий к обсуждению жалобы"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintDiscussionComment : PropertyChangedBase, IDomainObject
	{
		public int Id { get; set; }

		private ComplaintDiscussion complaintDiscussion;
		[Display(Name = "Обсуждение жалобы")]
		public virtual ComplaintDiscussion ComplaintDiscussion {
			get => complaintDiscussion;
			set => SetField(ref complaintDiscussion, value, () => ComplaintDiscussion);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}
	}
}

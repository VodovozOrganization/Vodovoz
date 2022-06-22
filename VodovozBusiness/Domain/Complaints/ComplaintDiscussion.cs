using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
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
		public virtual int Id { get; set; }

		private Complaint complaint;
		[Display(Name = "Рекламация")]
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

		private DateTime startSubdivisionDate;
		[Display(Name = "Дата подключения подразделения")]
		public virtual DateTime StartSubdivisionDate
		{
			get => startSubdivisionDate;
			set => SetField(ref startSubdivisionDate, value, () => StartSubdivisionDate);
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

		IList<ComplaintDiscussionComment> comments = new List<ComplaintDiscussionComment>();
		[Display(Name = "Комментарии")]
		public virtual IList<ComplaintDiscussionComment> Comments {
			get => comments;
			set => SetField(ref comments, value, () => Comments);
		}

		GenericObservableList<ComplaintDiscussionComment> observableComments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintDiscussionComment> ObservableComments {
			get {
				if(observableComments == null)
					observableComments = new GenericObservableList<ComplaintDiscussionComment>(Comments);
				return observableComments;
			}
		}

		public ComplaintDiscussion() { }

		public virtual string Title => $"{GetType().GetSubjectName()} подразделения \"{Subdivision.Name}\"";
	}
}

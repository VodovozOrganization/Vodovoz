using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;

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
		private Complaint _complaint;
		private Subdivision _subdivision;
		private DateTime _startSubdivisionDate;
		private DateTime _plannedCompletionDate;
		private ComplaintDiscussionStatuses _status;
		private IList<ComplaintDiscussionComment> _comments = new List<ComplaintDiscussionComment>();
		private GenericObservableList<ComplaintDiscussionComment> _observableComments;

		public ComplaintDiscussion() { }

		public virtual int Id { get; set; }

		[Display(Name = "Рекламация")]
		public virtual Complaint Complaint
		{
			get => _complaint;
			set => SetField(ref _complaint, value);
		}

		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}

		[Display(Name = "Дата подключения подразделения")]
		public virtual DateTime StartSubdivisionDate
		{
			get => _startSubdivisionDate;
			set => SetField(ref _startSubdivisionDate, value);
		}

		[Display(Name = "Предполагаемая дата завершения")]
		public virtual DateTime PlannedCompletionDate
		{
			get => _plannedCompletionDate;
			set => SetField(ref _plannedCompletionDate, value);
		}

		[Display(Name = "Статус")]
		public virtual ComplaintDiscussionStatuses Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Комментарии")]
		public virtual IList<ComplaintDiscussionComment> Comments
		{
			get => _comments;
			set => SetField(ref _comments, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintDiscussionComment> ObservableComments
		{
			get
			{
				if(_observableComments == null)
				{
					_observableComments = new GenericObservableList<ComplaintDiscussionComment>(Comments);
				}

				return _observableComments;
			}
		}

		public virtual string Title => $"{typeof(ComplaintDiscussion).GetSubjectName()} подразделения \"{Subdivision.Name}\"";
	}
}

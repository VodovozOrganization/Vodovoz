using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using VodovozBusiness.Domain.Discussions;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "обсуждения",
		Nominative = "обсуждение"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintDiscussion
		: PropertyChangedBase,
		IDomainObject,
		IDiscussion
	{
		private Complaint _container;
		private Subdivision _subdivision;
		private DateTime _startSubdivisionDate;
		private DateTime _plannedCompletionDate;
		private ComplaintDiscussionStatuses _status;
		private IObservableList<ComplaintDiscussionComment> _comments = new ObservableList<ComplaintDiscussionComment>();

		public ComplaintDiscussion() { }

		public virtual int Id { get; set; }

		[Display(Name = "Рекламация")]
		public virtual Complaint Container
		{
			get => _container;
			set => SetField(ref _container, value);
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
		public virtual IObservableList<ComplaintDiscussionComment> Comments
		{
			get => _comments;
			set => SetField(ref _comments, value);
		}

		public virtual string Title => $"{typeof(ComplaintDiscussion).GetSubjectName()} подразделения \"{Subdivision.Name}\"";

		IDomainObject IDiscussion.Container
		{
			get => Container;
			set => Container = value as Complaint;
		}

		IObservableList<IDiscussionComment<DiscussionCommentFileInformation>> IDiscussion.Comments => Comments as IObservableList<IDiscussionComment<DiscussionCommentFileInformation>>;

		public virtual void AddComment(ComplaintDiscussionComment discussionComment)
		{
			discussionComment.Container = this;
			Comments.Add(discussionComment);
		}

		public virtual void AddComment(IDiscussionComment<DiscussionCommentFileInformation> discussionComment)
		{
			AddComment(discussionComment as ComplaintDiscussionComment);
		}
	}
}

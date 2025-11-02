using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "обсуждения недовоза",
		Nominative = "обсуждение недовоза"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveryDiscussion : PropertyChangedBase, IDomainObject
	{
		private UndeliveredOrder _undelivery;
		private Subdivision _subdivision;
		private DateTime _startSubdivisionDate;
		private DateTime _plannedCompletionDate;
		private UndeliveryDiscussionStatus _status;
		private IList<UndeliveryDiscussionComment> _comments = new List<UndeliveryDiscussionComment>();
		private GenericObservableList<UndeliveryDiscussionComment> _observableComments;

		public virtual int Id { get; set; }

		[Display(Name = "Недовоз")]
		public virtual UndeliveredOrder Undelivery
		{
			get => _undelivery;
			set => SetField(ref _undelivery, value);
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
		public virtual UndeliveryDiscussionStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Комментарии")]
		public virtual IList<UndeliveryDiscussionComment> Comments
		{
			get => _comments;
			set => SetField(ref _comments, value);
		}

		//FIXME Костлыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<UndeliveryDiscussionComment> ObservableComments =>
			_observableComments ?? (_observableComments = new GenericObservableList<UndeliveryDiscussionComment>(Comments));

		public virtual string Title => $"{typeof(UndeliveryDiscussion).GetSubjectName()} подразделения \"{Subdivision.Name}\"";
	}
}

using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "комментарии к обсуждению недовоза",
		Nominative = "комментарий к обсуждению недовоза"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveryDiscussionComment : PropertyChangedBase, IDomainObject
	{
		private Employee _author;
		private DateTime _creationTime;
		private UndeliveryDiscussion _undeliveryDiscussion;
		private string _comment;

		public virtual int Id { get; set; }


		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		[Display(Name = "Обсуждение недовоза")]
		public virtual UndeliveryDiscussion UndeliveryDiscussion
		{
			get => _undeliveryDiscussion;
			set => SetField(ref _undeliveryDiscussion, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value, () => Comment);
		}

		public virtual string Title => $"Комментарий сотрудника \"{Author.ShortName}\"";
	}
}

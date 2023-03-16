using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "комментарии результатов рекламации",
		Nominative = "комментарий результата рекламации"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintResultComment : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Complaint _complaint;
		[Display(Name = "Рекламация")]
		public virtual Complaint Complaint
		{
			get => _complaint;
			set => SetField(ref _complaint, value);
		}

		private Employee _author;
		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		private string _comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		private DateTime _creationTime;
		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value, () => CreationTime);
		}

		public virtual string Title => $"Комментарий результата сотрудника \"{Author.ShortName}\"";
	}
}

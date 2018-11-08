using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Chats
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "последние прочитанные сообщения",
		Nominative = "последнее прочитанное сообщение")]
	public class LastReadedMessage : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Chat chat;

		[Display (Name = "Чат")]
		public virtual Chat Chat {
			get { return chat; }
			set { SetField (ref chat, value, () => Chat); }
		}

		Employee employee;

		[Display (Name = "Пользователь")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField (ref employee, value, () => Employee); }
		}

		DateTime lastDateTime;

		[Display (Name = "Дата и время")]
		public virtual DateTime LastDateTime {
			get { return lastDateTime; }
			set { SetField (ref lastDateTime, value, () => LastDateTime); }
		}
	}
}

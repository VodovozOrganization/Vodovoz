using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Chats
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "сообщения",
		Nominative = "сообщение")]
	public class ChatMessage : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Chat chat;

		[Display(Name = "Чат")]
		public virtual Chat Chat {
			get { return chat; }
			set { SetField(ref chat, value, () => Chat); }
		}

		Employee sender;

		[Display(Name = "Отправитель")]
		public virtual Employee Sender {
			get { return sender; }
			set { SetField(ref sender, value, () => Sender); }
		}

		private bool isServerNotification;

		public virtual bool IsServerNotification {
			get { return isServerNotification; }
			set { SetField(ref isServerNotification, value, () => IsServerNotification); }
		}

		private bool isAutoCeated;

		[Display(Name = "Автоматически созданное сообщение")]
		public virtual bool IsAutoCeated {
			get { return isAutoCeated; }
			set { SetField(ref isAutoCeated, value, () => IsAutoCeated); }
		}

		string message;

		[Display(Name = "Сообщение")]
		public virtual string Message {
			get { return message; }
			set { SetField(ref message, value, () => Message); }
		}

		DateTime dateTime;

		[Display(Name = "Дата и время отправки")]
		public virtual DateTime DateTime {
			get { return dateTime; }
			set { SetField(ref dateTime, value, () => DateTime); }
		}

		#region Генерируемые

		public virtual string SenderName {
			get {
				return Sender?.ShortName ?? "незнамо кто";
			}
		}
		#endregion
	}
}


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Chats
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "чаты",
		Nominative = "чат")]
	public class Chat : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private ChatType chatType;

		[Display (Name = "Тип чата")]
		public virtual ChatType ChatType {
			get { return chatType; }
			set { SetField (ref chatType, value, () => ChatType); }
		}

		Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField (ref driver, value, () => Driver); }
		}

		IList<ChatMessage> messages = new List<ChatMessage> ();

		[Display (Name = "Сообщения чата")]
		public virtual IList<ChatMessage> Messages {
			get { return messages; }
			set {
				SetField (ref messages, value, () => Messages);
			}
		}

		GenericObservableList<ChatMessage> observableMessages;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ChatMessage> ObservableMessages {
			get {
				if (observableMessages == null) {
					observableMessages = new GenericObservableList<ChatMessage> (messages);
				}
				return observableMessages;
			}
		}

		IList<LastReadedMessage> lastReaded = new List<LastReadedMessage> ();

		[Display (Name = "Последние прочитанные сообщения чата")]
		public virtual IList<LastReadedMessage> LastReaded {
			get { return lastReaded; }
			set {
				SetField (ref lastReaded, value, () => LastReaded);
			}
		}

		GenericObservableList<LastReadedMessage> observableLastReaded;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<LastReadedMessage> ObservableLastReaded {
			get {
				if (observableLastReaded == null) {
					observableLastReaded = new GenericObservableList<LastReadedMessage> (lastReaded);
				}
				return observableLastReaded;
			}
		}

		#region Функции

		public virtual void UpdateLastReadedTime(Employee employee)
		{
			var lastReadedTime = LastReaded.FirstOrDefault (x => x.Employee.Id == employee.Id);
			if (lastReadedTime == null) {
				lastReadedTime = new LastReadedMessage ();
				lastReadedTime.Chat = this;
				lastReadedTime.Employee = employee;
				LastReaded.Add (lastReadedTime);
			}
			lastReadedTime.LastDateTime = DateTime.Now;
		}

		#endregion
	}

	public enum ChatType
	{
		[Display (Name = "Чат с водителем")]
		DriverAndLogists
	}

	public class ChatTypeStringType : NHibernate.Type.EnumStringType
	{
		public ChatTypeStringType () : base (typeof(ChatType))
		{
		}
	}
}


using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Электронные почты для отправки",
	    Nominative = "Электронная почта для отправки"
	)]
	public class StoredEmail : BusinessObjectBase<StoredEmail>, IDomainObject
	{
		private string _subject;
		private Guid? _guid;

		public virtual int Id { get; set; }

		public virtual string ExternalId { get; set; }

		private DateTime sendDate;
		[Display(Name = "Дата действия")]
		public virtual DateTime SendDate {
			get { return sendDate; }
			set { SetField(ref sendDate, value, () => SendDate); }
		}

		private StoredEmailStates state;
		[Display(Name = "Состояние")]
		public virtual StoredEmailStates State {
			get { return state; }
			set { SetField(ref state, value, () => State); }
		}

		private DateTime stateChangeDate;
		[Display(Name = "Дата действия")]
		public virtual DateTime StateChangeDate {
			get { return stateChangeDate; }
			set { SetField(ref stateChangeDate, value, () => StateChangeDate); }
		}

		private string description;
		[Display(Name = "Описание")]
		public virtual string Description {
			get { return description; }
			set { SetField(ref description, value, () => Description); }
		}

		private string recipientAddress;
		[Display(Name = "Почта получателя")]
		public virtual string RecipientAddress {
			get { return recipientAddress; }
			set { SetField(ref recipientAddress, value, () => RecipientAddress); }
		}

		private bool? manualSending;
		[Display(Name = "Отправлено вручную")]
		public virtual bool? ManualSending {
			get { return manualSending; }
			set { SetField(ref manualSending, value, () => ManualSending); }
		}

		private Employee author;
		[Display(Name = "Автор")]
		public virtual Employee Author {
			get { return author; }
			set { SetField(ref author, value, () => Author); }
		}

		[Display(Name = "Тема")]
		public virtual string Subject
		{
			get { return _subject; }
			set { SetField(ref _subject, value); }
		}

		[Display(Name = "Guid")]
		public virtual Guid? Guid
		{
			get { return _guid; }
			set { SetField(ref _guid, value); }
		}

		public virtual void AddDescription(string description)
		{
			if(!string.IsNullOrWhiteSpace(Description)){
				Description += "\n";
			}
			Description += description;
		}
	}

	public enum StoredEmailStates
	{
		[Display(Name = "Подготовка к отправке")]
		PreparingToSend,
		[Display(Name = "Ожидание отправки")]
		WaitingToSend,
		[Display(Name = "Ошибка отправки")]
		SendingError,
		[Display(Name = "Успешно отправлено")]
		SendingComplete,
		[Display(Name = "Недоставлено")]
		Undelivered,
		[Display(Name = "Доставлено")]
		Delivered,
		[Display(Name = "Открыто")]
		Opened,
		[Display(Name = "Отмечено пользователем как спам")]
		MarkedAsSpam,
	}

	public class StoredEmailActionStatesStringType : NHibernate.Type.EnumStringType
	{
		public StoredEmailActionStatesStringType() : base(typeof(StoredEmailStates))
		{
		}
	}
}

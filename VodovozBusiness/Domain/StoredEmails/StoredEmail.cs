using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Domain.StoredEmails
{
	[OrmSubject(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Электронные почты для отправки",
	    Nominative = "Электронная почта для отправки"
	)]
	public class StoredEmail : BusinessObjectBase<StoredEmail>, IDomainObject
	{
		public virtual int Id { get; set; }

		private Order order;
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField(ref order, value, () => Order); }
		}

		private OrderDocumentType documentType;
		[Display(Name = "Тип документа")]
		public virtual OrderDocumentType DocumentType {
			get { return documentType; }
			set { SetField(ref documentType, value, () => DocumentType); }
		}

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

		private string htmlText;
		[Display(Name = "Текст письма в формате html")]
		public virtual string HtmlText {
			get { return htmlText; }
			set { SetField(ref htmlText, value, () => HtmlText); }
		}

		private string text;
		[Display(Name = "Текст письма")]
		public virtual string Text {
			get { return text; }
			set { SetField(ref text, value, () => Text); }
		}

		private string title;
		[Display(Name = "Тема письма")]
		public virtual string Title {
			get { return title; }
			set { SetField(ref title, value, () => Title); }
		}

		private string senderName;
		[Display(Name = "Имя отправителя")]
		public virtual string SenderName {
			get { return senderName; }
			set { SetField(ref senderName, value, () => SenderName); }
		}

		private string senderAddress;
		[Display(Name = "Почта отправителя")]
		public virtual string SenderAddress {
			get { return senderAddress; }
			set { SetField(ref senderAddress, value, () => SenderAddress); }
		}

		private string recipientName;
		[Display(Name = "Имя получателя")]
		public virtual string RecipientName {
			get { return recipientName; }
			set { SetField(ref recipientName, value, () => RecipientName); }
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
		[Display(Name = "Ожидание отправки")]
		WaitingToSend,
		[Display(Name = "Ошибка")]
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

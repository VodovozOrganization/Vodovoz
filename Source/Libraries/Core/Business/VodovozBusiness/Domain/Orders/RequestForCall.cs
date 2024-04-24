using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Заявки на звонок",
		Nominative = "Заявка на звонок",
		Prepositional = "Заявке на звонок",
		PrepositionalPlural = "Заявках на звонок"
	)]
	[HistoryTrace]
	public class RequestForCall : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Source _source;
		private int _externalId;
		private string _author;
		private string _phone;
		private string _question;
		private Counterparty _counterparty;
		private Nomenclature _nomenclature;
		private Employee _employeeWorkWith;
		private Order _order;
		private RequestForCallStatus _requestForCallStatus;
		private DateTime _created;
		private RequestForCallClosedReason _closedReason;

		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime Created
		{
			get => _created;
			set => SetField(ref _created, value);
		}
		
		[Display(Name = "Номер заявки из ИПЗ")]
		public virtual int ExternalId
		{
			get => _externalId;
			set => SetField(ref _externalId, value);
		}
		
		[Display(Name = "Источник заявки")]
		public virtual Source Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}
		
		[Display(Name = "Автор заявки")]
		public virtual string Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}
		
		[Display(Name = "Вопрос")]
		public virtual string Question
		{
			get => _question;
			set => SetField(ref _question, value);
		}
		
		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		[Display(Name = "Номер телефона")]
		public virtual string Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}
		
		[Display(Name = "Товар")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
		
		[Display(Name = "У кого в работе заявка")]
		public virtual Employee EmployeeWorkWith
		{
			get => _employeeWorkWith;
			set => SetField(ref _employeeWorkWith, value);
		}
		
		[Display(Name = "Оформленный заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
		
		[Display(Name = "Статус заявки")]
		public virtual RequestForCallStatus RequestForCallStatus
		{
			get => _requestForCallStatus;
			set => SetField(ref _requestForCallStatus, value);
		}
		
		[Display(Name = "Причина закрытия")]
		public virtual RequestForCallClosedReason ClosedReason
		{
			get => _closedReason;
			set => SetField(ref _closedReason, value);
		}
		
		public virtual void AttachOrder(Order order)
		{
			Order = order;
			RequestForCallStatus = RequestForCallStatus.OrderPerformed;
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(ClosedReason != null && RequestForCallStatus != RequestForCallStatus.Closed)
			{
				yield return new ValidationResult(
					"Неправильное состояние заявки. Если указана причина закрытия, то заявку нужно закрыть");
			}
		}
	}
}

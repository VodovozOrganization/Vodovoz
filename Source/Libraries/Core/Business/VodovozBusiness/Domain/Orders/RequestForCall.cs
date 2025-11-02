using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Заявки на звонок",
		Nominative = RequestForCallName,
		Prepositional = "Заявке на звонок",
		PrepositionalPlural = "Заявках на звонок"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class RequestForCall : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public const string RequestForCallName = "Заявка на звонок";
		private Source _source;
		private string _author;
		private string _phone;
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
			protected set => SetField(ref _created, value);
		}
		
		[Display(Name = "Источник заявки")]
		public virtual Source Source
		{
			get => _source;
			protected set => SetField(ref _source, value);
		}
		
		[Display(Name = "Автор заявки")]
		public virtual string Author
		{
			get => _author;
			protected set => SetField(ref _author, value);
		}
		
		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			protected set => SetField(ref _counterparty, value);
		}
		
		[Display(Name = "Номер телефона")]
		public virtual string Phone
		{
			get => _phone;
			protected set => SetField(ref _phone, value);
		}
		
		[Display(Name = "Товар")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			protected set => SetField(ref _nomenclature, value);
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

		public static RequestForCall Create(
			Source source,
			string contactName,
			string phoneNumber,
			Nomenclature nomenclature,
			Counterparty counterparty)
		{
			var requestForCall = new RequestForCall
			{
				Source = source,
				Author = contactName,
				Phone = phoneNumber,
				Nomenclature = nomenclature,
				Counterparty = counterparty,
				Created = DateTime.Now,
				RequestForCallStatus = RequestForCallStatus.New
			};

			return requestForCall;
		}

		public override string ToString() => Id == 0 ? $"Новая { RequestForCallName.ToLower() }" : RequestForCallName;

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

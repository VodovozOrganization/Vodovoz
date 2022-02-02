using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Документы заказа для электронной почты",
		Nominative = "Документ заказа для электронной почты"
	)]
	public abstract class CounterpartyEmail : PropertyChangedBase, IDomainObject
	{
		private Order _order;
		private StoredEmail _storedEmail;
		private OrderDocument _orderDocument;
		private Counterparty _counterparty;

		public virtual int Id { get; set; }

		[Display(Name = "Электронная почта для отправки")]
		public virtual StoredEmail StoredEmail
		{
			get => _storedEmail;
			set => SetField(ref _storedEmail, value);
		}


		[Display(Name = "Тип почтового отправления")]
		public virtual CounterpartyEmailType Type { get; set; }

		public abstract string CounterpartyFullName { get; }
		
		public abstract IEmailableDocument EmailableDocument { get; }
	}

	public enum CounterpartyEmailType
	{
		[Display(Name = "Документ заказа")]
		OrderDocument,
		[Display(Name = "Долг")]
		DebtBill,
		[Display(Name = "Массовая рассылка")]
		Bulk,
		[Display(Name = "Учётные данные")]
		Credential,
		[Display(Name = "Счёт без отгрузки на долг")]
		OrderWithoutShipmentForDebt,
		[Display(Name = "Счёт без отгрузки на постоплату")]
		OrderWithoutShipmentForPayment,
		[Display(Name = "Счёт без отгрузки на предоплату")]
		OrderWithoutShipmentForAdvancePayment
	}

	public class CounterpartyEmailStringType : NHibernate.Type.EnumStringType
	{
		public CounterpartyEmailStringType() : base(typeof(CounterpartyEmailType)) { }
	}
}

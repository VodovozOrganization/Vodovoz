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
		NominativePlural = "Документы заказа для электронной почты",
		Nominative = "Документ заказа для электронной почты"
	)]
	public class OrderDocumentEmail : BusinessObjectBase<OrderDocumentEmail>, IDomainObject
	{
		private Order _order;
		private OrderDocumentType _documentType;
		private StoredEmail _storedEmail;
		private OrderDocument _orderDocument;

		public virtual int Id { get; set; }

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Тип документа")]
		public virtual OrderDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		[Display(Name = "Электронная почта для отправки")]
		public virtual StoredEmail StoredEmail
		{
			get => _storedEmail;
			set => SetField(ref _storedEmail, value);
		}

		[Display(Name = "Документ заказа для отправки")]
		public virtual OrderDocument OrderDocument
		{
			get => _orderDocument;
			set => SetField(ref _orderDocument, value);
		}
	}
}

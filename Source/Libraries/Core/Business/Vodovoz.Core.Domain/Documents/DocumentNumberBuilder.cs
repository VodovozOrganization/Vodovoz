using System;
using System.Linq;
using NHibernate.Criterion;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Documents
{
	public class DocumentNumberBuilder
	{
		private static readonly DateTime _dateForNewDocumentNumbers = new DateTime(2026, 1, 1);

		public static string Build(OrderEntity order, DocumentContainerType documentContainerType)
		{
			var documentTypes = documentContainerType == DocumentContainerType.Upd
				? new[] { OrderDocumentType.UPD, OrderDocumentType.SpecialUPD }
				: new[] { OrderDocumentType.Bill, OrderDocumentType.SpecialBill };

			if(order.DeliveryDate < _dateForNewDocumentNumbers)
			{
				return order.Id.ToString();
			}

			if(order?.OrderDocuments == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var document = order.OrderDocuments
				.FirstOrDefault(x => documentTypes.Contains(x.Type) && x.Order.Id == order.Id);

			if(document == null)
			{
				throw new InvalidOperationException($"Нет документа для заказа #{order.Id}, кол-во документов {order.OrderDocuments.Count}");
			}

			return document.DocumentOrganizationCounter?.DocumentNumber
				   ?? throw new InvalidOperationException("Номер документа отсутствует");
		}

		public static string Build(
			TransferOrder transferOrder, 
			DocumentOrganizationCounter documentOrganizationCounter)
		{
			if(transferOrder.Date < _dateForNewDocumentNumbers)
			{
				return transferOrder.Id.ToString();
			}

			if(documentOrganizationCounter == null)
			{
				throw new InvalidOperationException("Нет документа-счетчика для трансфера заказа");
			}
			
			return documentOrganizationCounter.DocumentNumber;
		}
		
		public static string BuildDocumentNumber(
			OrganizationEntity org, 
			DateTime date, 
			int counter)
			=> $"{org.Prefix}{date:yy}-{counter}";
	}
}

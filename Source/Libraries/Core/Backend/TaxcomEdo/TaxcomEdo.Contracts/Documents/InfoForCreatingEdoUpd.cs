using System.Collections.Generic;
using TaxcomEdo.Contracts.Orders;
using TaxcomEdo.Contracts.Payments;

namespace TaxcomEdo.Contracts.Documents
{
	public class InfoForCreatingEdoUpd : InfoForCreatingDocumentEdo
	{
		protected InfoForCreatingEdoUpd(OrderInfoForEdo orderInfoForEdo, IEnumerable<PaymentInfoForEdo> paymentsInfoForEdo)
		{
			OrderInfoForEdo = orderInfoForEdo;
			PaymentsInfoForEdo = paymentsInfoForEdo;
		}
		
		public OrderInfoForEdo OrderInfoForEdo { get; }
		public IEnumerable<PaymentInfoForEdo> PaymentsInfoForEdo { get; }

		public static InfoForCreatingEdoUpd Create(OrderInfoForEdo orderInfoForEdo, IEnumerable<PaymentInfoForEdo> paymentsInfoForEdo) =>
			new InfoForCreatingEdoUpd(orderInfoForEdo, paymentsInfoForEdo);
	}
}

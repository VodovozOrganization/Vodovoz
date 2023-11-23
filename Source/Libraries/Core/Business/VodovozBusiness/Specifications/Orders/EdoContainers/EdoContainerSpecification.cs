using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Specifications.Orders.EdoContainers
{
	public class EdoContainerSpecification : ExpressionSpecification<EdoContainer>
	{
		private EdoContainerSpecification(Expression<Func<EdoContainer, bool>> expression)
			: base(expression)
		{
		}

		public static EdoContainerSpecification CreateForOrderId(int orderId)
			=> new EdoContainerSpecification(x => x.Order.Id == orderId);

		public static EdoContainerSpecification CreateForOrderWithoutShipmentForAdvancePaymentId(int orderWithoutShipmentForAdvancePaymentId)
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForAdvancePayment.Id == orderWithoutShipmentForAdvancePaymentId);

		public static EdoContainerSpecification CreateForOrderWithoutShipmentForDebtId(int orderWithoutShipmentForDebtId)
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForDebt.Id == orderWithoutShipmentForDebtId);

		public static EdoContainerSpecification CreateForOrderWithoutShipmentForPaymentId(int orderWithoutShipmentForPaymentId)
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForPayment.Id == orderWithoutShipmentForPaymentId);
	}
}

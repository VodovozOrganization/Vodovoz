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

		public static ExpressionSpecification<EdoContainer> CreateForOrderId(int orderId)
			=> new EdoContainerSpecification(x => x.Order.Id == orderId);

		public static ExpressionSpecification<EdoContainer> CreateForOrderWithoutShipmentForAdvancePaymentId(int orderWithoutShipmentForAdvancePaymentId)
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForAdvancePayment.Id == orderWithoutShipmentForAdvancePaymentId);

		public static ExpressionSpecification<EdoContainer> CreateForOrderWithoutShipmentForDebtId(int orderWithoutShipmentForDebtId)
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForDebt.Id == orderWithoutShipmentForDebtId);

		public static ExpressionSpecification<EdoContainer> CreateForOrderWithoutShipmentForPaymentId(int orderWithoutShipmentForPaymentId)
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForPayment.Id == orderWithoutShipmentForPaymentId);

		public static ExpressionSpecification<EdoContainer> CreateIsForOrder()
			=> new EdoContainerSpecification(x => x.Order != null);

		public static ExpressionSpecification<EdoContainer> CreateIsForOrderWithoutShipmentForAdvancePayment()
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForAdvancePayment != null);

		public static ExpressionSpecification<EdoContainer> CreateIsForOrderWithoutShipmentForDebt()
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForDebt != null);

		public static ExpressionSpecification<EdoContainer> CreateIsForOrderWithoutShipmentForPayment()
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForPayment != null);

		public static ExpressionSpecification<EdoContainer> CreateForCreatedAfter(DateTime dateTime)
			=> new EdoContainerSpecification(x => x.Created >= dateTime);

		public static ExpressionSpecification<EdoContainer> CreateForStatus(EdoDocFlowStatus edoDocFlowStatus)
			=> new EdoContainerSpecification(x => x.EdoDocFlowStatus == edoDocFlowStatus);

		public static ExpressionSpecification<EdoContainer> CreateForOrganizationId(int organizationId)
			=> CreateForOrderContractOrganizationId(organizationId);

		public static ExpressionSpecification<EdoContainer> CreateForOrderContractOrganizationId(int organizationId)
			=> new EdoContainerSpecification(x => x.Order.Contract.Organization.Id == organizationId);

		public static ExpressionSpecification<EdoContainer> CreateForAftedDateNotSendedWithOrganizationId(DateTime dateTime, int organizationId) =>
			CreateForCreatedAfter(dateTime)
			.And(CreateForStatus(EdoDocFlowStatus.PreparingToSend))
			.And(CreateForOrganizationId(organizationId)
				.Or(CreateIsForOrderWithoutShipmentForAdvancePayment())
				.Or(CreateIsForOrderWithoutShipmentForDebt())
				.Or(CreateIsForOrderWithoutShipmentForPayment()));
	}
}

using System;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Specifications;
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

		public static EdoContainerSpecification CreateIsForOrder()
			=> new EdoContainerSpecification(x => x.Order != null);

		public static EdoContainerSpecification CreateIsForOrderWithoutShipmentForAdvancePayment()
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForAdvancePayment != null);

		public static EdoContainerSpecification CreateIsForOrderWithoutShipmentForDebt()
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForDebt != null);

		public static EdoContainerSpecification CreateIsForOrderWithoutShipmentForPayment()
			=> new EdoContainerSpecification(x => x.OrderWithoutShipmentForPayment != null);

		public static EdoContainerSpecification CreateForCreatedAfter(DateTime dateTime)
			=> new EdoContainerSpecification(x => x.Created >= dateTime);

		public static EdoContainerSpecification CreateForStatus(EdoDocFlowStatus edoDocFlowStatus)
			=> new EdoContainerSpecification(x => x.EdoDocFlowStatus == edoDocFlowStatus);

		public static EdoContainerSpecification CreateForOrderContractOrganizationId(int organizationId)
			=> new EdoContainerSpecification(x => x.Order.Contract.Organization.Id == organizationId);

		public static ExpressionSpecification<EdoContainer> CreateForOrganizationId(int organizationId)
			=> CreateForOrderContractOrganizationId(organizationId);

		public static ExpressionSpecification<EdoContainer> CreateForAftedDateNotSendedWithOrganizationId(DateTime dateTime, int organizationId)
			=> CreateForCreatedAfter(dateTime)
				.And(CreateForStatus(EdoDocFlowStatus.PreparingToSend))
				.And(CreateForOrganizationId(organizationId)
					.Or(CreateIsForOrderWithoutShipmentForAdvancePayment())
					.Or(CreateIsForOrderWithoutShipmentForDebt())
					.Or(CreateIsForOrderWithoutShipmentForPayment()));
	}
}

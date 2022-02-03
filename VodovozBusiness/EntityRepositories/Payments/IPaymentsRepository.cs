using System;
using System.Collections.Generic;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;
using Vodovoz.Services;

namespace Vodovoz.EntityRepositories.Payments
{
	public interface IPaymentsRepository
	{
		IList<PaymentByCardOnlineNode> GetPaymentsByTwoMonths(IUnitOfWork uow, DateTime date);
		IEnumerable<string> GetAllShopsFromTinkoff(IUnitOfWork uow);
		bool PaymentFromBankClientExists(
			IUnitOfWork uow, DateTime date, int number, string organisationInn, string counterpartyInn, string accountNumber);
		decimal GetCounterpartyLastBalance(IUnitOfWork uow, int counterpartyId);
		IList<Payment> GetAllUndistributedPayments(IUnitOfWork uow, IProfitCategoryProvider profitCategoryProvider);
		IList<Payment> GetAllDistributedPayments(IUnitOfWork uow);
		Payment GetRefundPayment(IUnitOfWork uow, int refundedPaymentId);
		IList<NotFullyAllocatedPaymentNode> GetAllNotFullyAllocatedPaymentsByClientAndOrg(
			IUnitOfWork uow, int counterpartyId, int organizationId);
		IQueryOver<Payment, Payment> GetAllUnAllocatedBalances(IUnitOfWork uow, int closingDocumentDeliveryScheduleId);
	}
}

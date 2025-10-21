using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Payments;

namespace VodovozBusiness.Services
{
	public interface IPaymentService
	{
		/// <summary>
		/// Отмена распределения
		/// </summary>
		/// <param name="payment">Оплата по которой отменяется распределение</param>
		/// <param name="cancellationReason">Причина отмены</param>
		/// <param name="isByUserRequest">Пользовательский запрос или автоматика</param>
		void CancelAllocation(IUnitOfWork uow, Payment payment, string cancellationReason, bool isByUserRequest);
		void CancelAllocationWithUpdateOrderPayments(IUnitOfWork uow, PaymentItem paymentItem);
		Result DistributeByClientIdAndOrganizationId(IUnitOfWork unitOfWork, int counterpartyId, int organizationId, bool distributeCompletedPayments = false);
		Result<IEnumerable<UnallocatedBalancesJournalNode>> GetAllUnallocatedBalancesForAutomaticDistribution(IUnitOfWork unitOfWork);
	}
}

using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IRefundOperationRepository
	{
		/// <summary>
		/// Получить успешный возврат по ID транзакции
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="transactionId"></param>
		/// <param name="paymentSource"></param>
		/// <returns></returns>
		RefundOperation GetSuccessfulByTransactionId(
			IUnitOfWork uow,
			string transactionId,
			OnlinePaymentSource? paymentSource);

		/// <summary>
		/// Получить последнюю попытку возврата по транзакции
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="transactionId"></param>
		/// <param name="paymentSource"></param>
		/// <returns></returns>
		RefundOperation GetLastAttemptByTransactionId(
			IUnitOfWork uow,
			string transactionId,
			OnlinePaymentSource? paymentSource);
	}
}

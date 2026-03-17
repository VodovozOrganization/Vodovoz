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
		/// <param name="uow">Unit of Work</param>
		/// <param name="transactionId">Идентификатор транзакции в платежной системе</param>
		/// <param name="paymentSource">Источник платежа</param>
		/// <returns>Операция возврата со статусом "Успешно"</returns>
		RefundOperation GetSuccessfulByTransactionId(
			IUnitOfWork uow,
			string transactionId,
			OnlinePaymentSource? paymentSource);

		/// <summary>
		/// Получить последнюю попытку возврата по транзакции
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="transactionId">Идентификатор транзакции в платежной системе</param>
		/// <param name="paymentSource">Источник платежа</param>
		/// <returns>Последняя по времени операция возврата для указанной транзакции</returns>
		/// <remarks>Возвращает самую свежую операцию возврата вне зависимости от её статуса</remarks>
		RefundOperation GetLastAttemptByTransactionId(
			IUnitOfWork uow,
			string transactionId,
			OnlinePaymentSource? paymentSource);
	}
}

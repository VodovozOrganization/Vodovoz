using CloudPaymentsApi.Library.Models;
using CloudPaymentsApi.Library.Requests;
using CloudPaymentsApi.Library.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace CloudPaymentsApi.Client
{
	/// <summary>
	/// Клиент для взаимодействия с API CloudPayments
	/// </summary>
	public interface ICloudPaymentsApiClient
	{
		/// <summary>
		/// Получает информацию о транзакции по её идентификатору
		/// </summary>
		/// <param name="transactionId">Идентификатор транзакции в системе CloudPayments</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Объект ответа <see cref="CloudPaymentsResponse{T}"/> с данными о транзакции <see cref="CloudPaymentsTransaction"/>
		/// </returns>
		Task<CloudPaymentsResponse<CloudPaymentsTransaction>> GetTransactionAsync(long transactionId, CancellationToken cancellationToken);

		/// <summary>
		/// Выполняет возврат средств по транзакции
		/// </summary>
		/// <param name="request">Параметры запроса на возврат средств <see cref="CloudPaymentsRefundRequest"/></param>
		/// <param name="idempotenceKey">Ключ идемпотентности</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Объект ответа <see cref="CloudPaymentsResponse{T}"/> с результатом возврата <see cref="CloudPaymentsRefundResult"/>
		/// </returns>
		Task<CloudPaymentsResponse<CloudPaymentsRefundResult>> RefundAsync(CloudPaymentsRefundRequest request, string idempotenceKey, CancellationToken cancellationToken);
	}
}

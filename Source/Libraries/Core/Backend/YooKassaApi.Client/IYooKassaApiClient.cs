using System.Threading;
using System.Threading.Tasks;
using YooKassaApi.Library.Models;
using YooKassaApi.Library.Requests;
using YooKassaApi.Library.Responses;

namespace YooKassaApi.Client
{
	/// <summary>
	/// API клиент для интеграции с ЮKassa
	/// </summary>
	public interface IYooKassaApiClient
	{
		/// <summary>
		/// Получает информацию о платеже по его идентификатору
		/// </summary>
		/// <param name="paymentId">Идентификатор платежа в системе ЮKassa</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>
		/// Объект результата <see cref="YooKassaResult{T}"/> с данными о платеже <see cref="YooKassaPaymentResponse"/>
		/// </returns>
		Task<YooKassaResult<YooKassaPaymentResponse>> GetPaymentAsync(string paymentId, CancellationToken cancellationToken);

		/// <summary>
		/// Выполняет возврат средств по платежу
		/// </summary>
		/// <param name="request">Запрос на возврат средств <see cref="YooKassaRefundRequest"/></param>
		/// <param name="idempotenceKey">Ключ идемпотентности</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>
		/// Объект результата <see cref="YooKassaResult{T}"/> с данными о возврате <see cref="YooKassaRefundResponse"/>
		/// </returns>
		Task<YooKassaResult<YooKassaRefundResponse>> RefundAsync(YooKassaRefundRequest request, string idempotenceKey, CancellationToken cancellationToken);
	}
}

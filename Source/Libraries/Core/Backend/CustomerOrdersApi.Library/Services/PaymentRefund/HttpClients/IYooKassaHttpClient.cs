using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients
{
	public interface IYooKassaHttpClient
	{
		/// <summary>
		/// Получить информацию о платеже по его ID
		/// </summary>
		/// <param name="paymentId">ID платежа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<YooKassaResult<YooKassaPaymentResponse>> GetPaymentAsync(string paymentId, CancellationToken cancellationToken);

		/// <summary>
		/// Выполнить возврат средств по платежу
		/// </summary>
		/// <param name="request">Запрос на возврат средств</param>
		/// <param name="idempotenceKey">Ключ идемпотентности</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<YooKassaResult<YooKassaRefundResponse>> RefundAsync(YooKassaRefundRequest request, string idempotenceKey, CancellationToken cancellationToken);
	}
}

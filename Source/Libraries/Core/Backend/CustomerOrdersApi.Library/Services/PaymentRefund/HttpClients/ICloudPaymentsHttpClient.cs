using CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients
{
	public interface ICloudPaymentsHttpClient
	{
		/// <summary>
		/// Получить информацию о транзакции по ее ID
		/// </summary>
		/// <param name="transactionId">ID транзакции</param>
		/// <returns></returns>
		Task<CloudPaymentsResponse<CloudPaymentsTransaction>> GetTransactionAsync(long transactionId);

		/// <summary>
		/// Возврат денег по транзакции
		/// </summary>
		/// <param name="request">Запрос на возврат</param>
		/// <returns></returns>
		Task<CloudPaymentsResponse<CloudPaymentsRefundResult>> RefundAsync(CloudPaymentsRefundRequest request);
	}
}

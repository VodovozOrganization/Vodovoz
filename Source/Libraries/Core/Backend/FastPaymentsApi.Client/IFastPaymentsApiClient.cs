using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace FastPaymentsApi.Client
{
	/// <summary>
	/// API клиент для интеграции с FastPayment Order API
	/// </summary>
	public interface IFastPaymentsApiClient
	{
		/// <summary>
		/// Выполняет возврат средств по заказу, оплаченному через быстрые платежи (QR-код)
		/// </summary>
		/// <param name="request">Запрос на возврат платежа <see cref="ReverseTicketRequestDTO"/>, содержащий идентификатор сессии (Ticket) и сумму возврата</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>
		/// Ответ от API <see cref="ReverseTicketResponseDTO"/> с результатом выполнения возврата
		/// </returns>
		Task<ReverseTicketResponseDTO> ReverseOrderAsync(ReverseTicketRequestDTO request, CancellationToken cancellationToken);
	}
}

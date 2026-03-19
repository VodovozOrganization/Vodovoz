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
		/// Выполнить возврат заказа
		/// </summary>
		Task<ReverseTicketResponseDTO> ReverseOrderAsync(ReverseTicketRequestDTO request, CancellationToken cancellationToken);
	}
}

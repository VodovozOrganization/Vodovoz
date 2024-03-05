using FastPaymentsApi.Contracts.Responses;
using System.Threading.Tasks;

namespace FastPaymentsAPI.Library.Services
{
	public interface IOrderService
	{
		Task<OrderRegistrationResponseDTO> RegisterOrderAsync(string xmlStringOrderRegistrationRequestDTO);
		Task<OrderInfoResponseDTO> GetOrderInfoAsync(string xmlStringOrderInfoDTO);
		Task<CancelPaymentResponseDTO> CancelPaymentAsync(string xmlStringFromCancelPaymentRequestDTO);
	}
}

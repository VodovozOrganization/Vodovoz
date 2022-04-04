using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Responses;

namespace FastPaymentsAPI.Library.Services;

public interface IOrderService
{
	Task<OrderRegistrationResponseDTO> RegisterOrderAsync(string xmlStringOrderRegistrationRequestDTO);
	Task<OrderInfoResponseDTO> GetOrderInfoAsync(string xmlStringOrderInfoDTO);
	Task<CancelPaymentResponseDTO> CancelPaymentAsync(string xmlStringFromCancelPaymentRequestDTO);
}

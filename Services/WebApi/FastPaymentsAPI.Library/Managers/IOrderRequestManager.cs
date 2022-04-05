using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Responses;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Managers
{
	public interface IOrderRequestManager
	{
		Task<OrderRegistrationResponseDTO> RegisterOrder(Order order, string phoneNumber = null);
		Task<OrderInfoResponseDTO> GetOrderInfo(string ticket);
		Task<CancelPaymentResponseDTO> CancelPayment(string ticket);
	}
}

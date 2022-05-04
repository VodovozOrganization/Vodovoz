using System;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.DTO_s.Responses;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Managers
{
	public interface IOrderRequestManager
	{
		string GetVodovozFastPayUrl(Guid fastPaymentGuid);
		Task<OrderRegistrationResponseDTO> RegisterOrder(Order order, Guid fastPaymentGuid, string phoneNumber = null);
		Task<OrderRegistrationResponseDTO> RegisterOnlineOrder(RequestRegisterOnlineOrderDTO registerOnlineOrderDto);
		Task<OrderInfoResponseDTO> GetOrderInfo(string ticket);
		Task<CancelPaymentResponseDTO> CancelPayment(string ticket);
	}
}

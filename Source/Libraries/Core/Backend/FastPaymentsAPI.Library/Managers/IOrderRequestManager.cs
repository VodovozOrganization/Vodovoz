using System;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.DTO_s.Responses;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsAPI.Library.Managers
{
	public interface IOrderRequestManager
	{
		string GetVodovozFastPayUrl(Guid fastPaymentGuid);
		Task<OrderRegistrationResponseDTO> RegisterOrder(
			Order order, Guid fastPaymentGuid, Organization organization, string phoneNumber = null, bool isQr = true);
		Task<OrderRegistrationResponseDTO> RegisterOnlineOrder
			(RequestRegisterOnlineOrderDTO registerOnlineOrderDto, Organization organization);
		Task<OrderInfoResponseDTO> GetOrderInfo(string ticket, Organization organization);
		Task<CancelPaymentResponseDTO> CancelPayment(string ticket, Organization organization);
	}
}

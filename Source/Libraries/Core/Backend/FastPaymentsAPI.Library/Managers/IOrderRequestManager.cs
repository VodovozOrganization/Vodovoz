using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using System;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsAPI.Library.Managers
{
	public interface IOrderRequestManager
	{
		string GetVodovozFastPayUrl(Guid fastPaymentGuid);
		Task<OrderRegistrationResponseDTO> RegisterOrder(
			Order order, Guid fastPaymentGuid, Organization organization, string phoneNumber = null, bool isQr = true);
		Task<OrderRegistrationResponseDTO> RegisterOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto,
			Organization organization,
			FastPaymentRequestFromType fastPaymentRequestFromType);
		Task<OrderInfoResponseDTO> GetOrderInfo(string ticket, Organization organization);
		Task<CancelPaymentResponseDTO> CancelPayment(string ticket, Organization organization);
	}
}

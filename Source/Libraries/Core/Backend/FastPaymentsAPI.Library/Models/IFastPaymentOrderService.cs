using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsAPI.Library.Models
{
	public interface IFastPaymentOrderService
	{
		Order GetOrder(int orderId);
		string ValidateParameters(int orderId);
		string ValidateParameters(RequestRegisterOnlineOrderDTO registerOnlineOrderDto, FastPaymentRequestFromType fastPaymentRequestFromType);
		string ValidateParameters(int orderId, ref string phoneNumber);
		string ValidateOrder(Order order, int orderId);
		string ValidateOnlineOrder(decimal onlineOrderSum);
		string GetPayUrlForOnlineOrder(Guid fastPaymentGuid);
		Task<OrderRegistrationResponseDTO> RegisterOrder(
			Order order, Guid fastPaymentGuid, Organization organization, string phoneNumber = null, bool isQr = true);
		Task<OrderRegistrationResponseDTO> RegisterOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto,
			Organization organization,
			FastPaymentRequestFromType fastPaymentRequestFromType);
		Task<OrderInfoResponseDTO> GetOrderInfo(string ticket, Organization organization);
		Task<CancelPaymentResponseDTO> CancelPayment(string ticket, Organization organization);
		void NotifyEmployee(string orderNumber, string bankSignature, long shopId, string paymentSignature);
		PaidOrderInfoDTO GetPaidOrderInfo(string data);
	}
}

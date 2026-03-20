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

		/// <summary>
		/// Отправить запрос на возврат денежных средств по платежу
		/// </summary>
		/// <param name="ticket">Тикет/сессия оплаты</param>
		/// <param name="organization">Организация</param>
		/// <param name="amount">Сумма возврата (опционально)</param>
		/// <returns>Результат возврата</returns>
		Task<ReverseOrderResponseDTO> ReverseOrder(string ticket, Organization organization, decimal? amount = null);
		Task NotifyEmployee(string orderNumber, string bankSignature, long shopId, string paymentSignature);
		PaidOrderInfoDTO GetPaidOrderInfo(string data);
	}
}

using System;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Responses;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Models
{
	public interface IFastPaymentOrderModel
	{
		Order GetOrder(int orderId);
		string ValidateParameters(int orderId);
		string ValidateParameters(int orderId, ref string phoneNumber);
		string ValidateOrder(Order order, int orderId);
		Task<OrderRegistrationResponseDTO> RegisterOrder(Order order, Guid fastPaymentGuid, string phoneNumber = null);
		Task<OrderInfoResponseDTO> GetOrderInfo(string ticket);
		Task<CancelPaymentResponseDTO> CancelPayment(string ticket);
		void NotifyEmployee(string orderNumber, string signature);
		PaidOrderInfoDTO GetPaidOrderInfo(string data);
	}
}

using System;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Managers;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Factories
{
	public interface IFastPaymentAPIFactory
	{
		OrderInfoRequestDTO GetOrderInfoRequestDTO(string ticket);
		OrderRegistrationRequestDTO GetOrderRegistrationRequestDTO(int orderId, string signature, decimal orderSum);
		OrderRegistrationRequestDTO GetOrderRegistrationRequestDTOForOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto, string signature);
		CancelPaymentRequestDTO GetCancelPaymentRequestDTO(string ticket);
		SignatureParams GetSignatureParamsForRegisterOrder(int orderId, decimal orderSum);
		SignatureParams GetSignatureParamsForValidate(PaidOrderInfoDTO paidOrderInfoDto);
		FastPayment GetFastPayment(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			DateTime creationDate,
			Guid fastPaymentGuid,
			decimal orderSum,
			Order order = null,
			string phoneNumber = null,
			int? onlineOrderId = null);
		FastPayment GetFastPayment(Order order, FastPaymentDTO paymentDto);
	}
}

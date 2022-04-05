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
		OrderRegistrationRequestDTO GetOrderRegistrationRequestDTO(int orderId, string signature, decimal orderSum, string backUrl);
		CancelPaymentRequestDTO GetCancelPaymentRequestDTO(string ticket);
		SignatureParams GetSignatureParamsForRegisterOrder(int orderId, decimal orderSum);
		SignatureParams GetSignatureParamsForValidate(PaidOrderInfoDTO paidOrderInfoDto);
		FastPayment GetFastPayment(
			OrderRegistrationResponseDTO orderRegistrationResponseDto, Order order, DateTime creationDate, string phoneNumber = null);
		FastPayment GetFastPayment(Order order, FastPaymentDTO paymentDto);
	}
}

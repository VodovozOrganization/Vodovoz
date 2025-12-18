using System;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Managers;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using VodovozInfrastructure;
using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library.Factories
{
	public interface IFastPaymentFactory
	{
		OrderInfoRequestDTO GetOrderInfoRequestDTO(string ticket, int shopId);
		OrderRegistrationRequestDTO GetOrderRegistrationRequestDTO(int orderId, string signature, decimal orderSum, int shopId);
		OrderRegistrationRequestDTO GetOrderRegistrationRequestDTOForOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto, string signature, int shopId);
		CancelPaymentRequestDTO GetCancelPaymentRequestDTO(string ticket, int shopId);
		OrderSignatureParams GetSignatureParamsForRegisterOrder(int orderId, decimal orderSum, int shopId);
		OrderSignatureParams GetSignatureParamsForValidate(PaidOrderInfoDTO paidOrderInfoDto);
		FastPayment GetFastPayment(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			DateTime creationDate,
			Guid fastPaymentGuid,
			decimal orderSum,
			int externalId,
			FastPaymentPayType payType,
			Organization organization,
			PaymentFrom paymentByCardFrom,
			PaymentType paymentType,
			FastPaymentRequestFromType fastPaymentRequestFromType,
			Order order = null,
			string phoneNumber = null,
			int? onlineOrderId = null,
			string callbackUrl = null);
		FastPayment GetFastPayment(Order order, FastPaymentDTO paymentDto);
		FastPaymentStatusChangeNotificationDto GetFastPaymentStatusChangeNotificationDto(FastPayment payment);
		OnlinePaymentDetailsDto GetNewOnlinePaymentDetailsDto(int onlineOrderId, decimal amount);
	}
}

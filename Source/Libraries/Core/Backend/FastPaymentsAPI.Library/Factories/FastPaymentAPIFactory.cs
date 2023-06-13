using System;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Managers;
using Microsoft.Extensions.Configuration;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsAPI.Library.Factories
{
	public class FastPaymentAPIFactory : IFastPaymentAPIFactory
	{
		private const string _signature = "Signature";
		private readonly IOrderSumConverter _orderSumConverter;
		private readonly IConfiguration _configuration;

		public FastPaymentAPIFactory(
			IConfiguration configuration,
			IOrderSumConverter orderSumConverter)
		{
			_orderSumConverter = orderSumConverter ?? throw new ArgumentNullException(nameof(orderSumConverter));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public OrderInfoRequestDTO GetOrderInfoRequestDTO(string ticket, int shopId)
		{
			var signatureSection = _configuration.GetSection($"{_signature}{shopId}");

			return new OrderInfoRequestDTO
			{
				Ticket = ticket,
				ShopId = signatureSection.GetValue<long>("ShopId"),
				ShopPasswd = signatureSection.GetValue<string>("ShopPasswd"),
			};
		}

		public OrderRegistrationRequestDTO GetOrderRegistrationRequestDTO(int orderId, string signature, decimal orderSum, int shopId)
		{
			var signatureSection = _configuration.GetSection($"{_signature}{shopId}");

			return new OrderRegistrationRequestDTO
			{
				ShopId = signatureSection.GetValue<long>("ShopId"),
				ShopPasswd = signatureSection.GetValue<string>("ShopPasswd"),
				Signature = signature,
				Amount = _orderSumConverter.ConvertOrderSumToKopecks(orderSum),
				OrderNumber = orderId.ToString(),
				OrderDescription = $"Заказ №{orderId}",
				Language = "RU"
			};
		}
		
		public OrderRegistrationRequestDTO GetOrderRegistrationRequestDTOForOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto, string signature, int shopId)
		{
			var signatureSection = _configuration.GetSection($"{_signature}{shopId}");

			return new OrderRegistrationRequestDTO
			{
				ShopId = signatureSection.GetValue<long>("ShopId"),
				ShopPasswd = signatureSection.GetValue<string>("ShopPasswd"),
				Signature = signature,
				Amount = _orderSumConverter.ConvertOrderSumToKopecks(registerOnlineOrderDto.OrderSum),
				OrderNumber = registerOnlineOrderDto.OrderId.ToString(),
				OrderDescription = $"Онлайн-заказ №{registerOnlineOrderDto.OrderId}",
				Language = "RU",
				IsQR = 1,
				BackUrl = registerOnlineOrderDto.BackUrl,
				BackUrlOk = registerOnlineOrderDto.BackUrlOk,
				BackUrlFail = registerOnlineOrderDto.BackUrlFail
			};
		}

		public CancelPaymentRequestDTO GetCancelPaymentRequestDTO(string ticket, int shopId)
		{
			var signatureSection = _configuration.GetSection($"{_signature}{shopId}");

			return new CancelPaymentRequestDTO
			{
				Ticket = ticket,
				ShopId = signatureSection.GetValue<long>("ShopId"),
				ShopPasswd = signatureSection.GetValue<string>("ShopPasswd"),
			};
		}

		public SignatureParams GetSignatureParamsForRegisterOrder(int orderId, decimal orderSum, int shopId)
		{
			var signatureSection = _configuration.GetSection($"{_signature}{shopId}");

			return new SignatureParams
			{
				ShopId = signatureSection.GetValue<long>("ShopId"),
				Sign = signatureSection.GetValue<string>("ShopSign"),
				OrderId = orderId.ToString(),
				OrderSumInKopecks = _orderSumConverter.ConvertOrderSumToKopecks(orderSum)
			};
		}

		public SignatureParams GetSignatureParamsForValidate(PaidOrderInfoDTO paidOrderInfoDto)
		{
			var shopId = paidOrderInfoDto.ShopId;
			var avSign = _configuration.GetSection($"{_signature}{shopId}").GetValue<string>("AvSign");

			return new SignatureParams
			{
				OrderId = paidOrderInfoDto.OrderNumber,
				OrderSumInKopecks = paidOrderInfoDto.Amount,
				ShopId = shopId,
				Sign = avSign
			};
		}

		public FastPayment GetFastPayment(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			DateTime creationDate,
			Guid fastPaymentGuid,
			decimal orderSum,
			int externalId,
			FastPaymentPayType payType,
			Organization organization,
			PaymentFrom paymentByCardFrom,
			PaymentType paymentType,
			Order order = null,
			string phoneNumber = null,
			int? onlineOrderId = null,
			string callbackUrl = null)
		{
			return new FastPayment
			{
				Amount = orderSum,
				CreationDate = creationDate,
				Order = order,
				Organization = organization,
				PaymentByCardFrom = paymentByCardFrom,
				PaymentType = paymentType,
				Ticket = orderRegistrationResponseDto.Ticket,
				QRPngBase64 = orderRegistrationResponseDto.QRPngBase64,
				ExternalId = externalId,
				PhoneNumber = phoneNumber,
				FastPaymentGuid = fastPaymentGuid,
				OnlineOrderId = onlineOrderId,
				FastPaymentPayType = payType,
				CallbackUrlForMobileApp = callbackUrl
			};
		}

		public FastPayment GetFastPayment(Order order, FastPaymentDTO paymentDto)
		{
			return new FastPayment
			{
				Amount = order.OrderSum,
				CreationDate = paymentDto.CreationDate,
				Order = order,
				Organization = paymentDto.Organization,
				PaymentByCardFrom = paymentDto.PaymentByCardFrom,
				Ticket = paymentDto.Ticket,
				QRPngBase64 = paymentDto.QRPngBase64,
				ExternalId = paymentDto.ExternalId,
				PhoneNumber = paymentDto.PhoneNumber,
				FastPaymentGuid = paymentDto.FastPaymentGuid,
				FastPaymentPayType = paymentDto.FastPaymentPayType
			};
		}

		public FastPaymentStatusChangeNotificationDto GetFastPaymentStatusChangeNotificationDto(
			int onlineOrderId, decimal amount, bool paymentSucceeded)
		{
			return new FastPaymentStatusChangeNotificationDto
			{
				PaymentDetails = GetNewOnlinePaymentDetailsDto(onlineOrderId, amount),
				PaymentStatus = paymentSucceeded ? PaymentStatusNotification.succeeded : PaymentStatusNotification.canceled
			};
		}

		public OnlinePaymentDetailsDto GetNewOnlinePaymentDetailsDto(int onlineOrderId, decimal amount)
		{
			return new OnlinePaymentDetailsDto
			{
				OnlineOrderId = onlineOrderId,
				PaymentSumDetails = GetNewOnlinePaymentSumDetailsDto(amount)
			};
		}

		private OnlinePaymentSumDetailsDto GetNewOnlinePaymentSumDetailsDto(decimal amount)
		{
			return new OnlinePaymentSumDetailsDto
			{
				PaymentSum = amount
			};
		}
	}
}

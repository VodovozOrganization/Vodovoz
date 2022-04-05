using System;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Managers;
using Microsoft.Extensions.Configuration;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;

namespace FastPaymentsAPI.Library.Factories
{
	public class FastPaymentAPIFactory : IFastPaymentAPIFactory
	{
		private readonly IOrderSumConverter _orderSumConverter;
		private readonly IConfigurationSection _signatureSection;

		public FastPaymentAPIFactory(
			IConfiguration configuration,
			IOrderSumConverter orderSumConverter)
		{
			_orderSumConverter = orderSumConverter ?? throw new ArgumentNullException(nameof(orderSumConverter));
			_signatureSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("Signature");
		}

		public OrderInfoRequestDTO GetOrderInfoRequestDTO(string ticket)
		{
			return new OrderInfoRequestDTO
			{
				Ticket = ticket,
				ShopId = _signatureSection.GetValue<long>("ShopId"),
				ShopPasswd = _signatureSection.GetValue<string>("ShopPasswd"),
			};
		}

		public OrderRegistrationRequestDTO GetOrderRegistrationRequestDTO(int orderId, string signature, decimal orderSum)
		{
			return new OrderRegistrationRequestDTO
			{
				ShopId = _signatureSection.GetValue<long>("ShopId"),
				ShopPasswd = _signatureSection.GetValue<string>("ShopPasswd"),
				Signature = signature,
				Amount = _orderSumConverter.ConvertOrderSumToKopecks(orderSum),
				OrderNumber = orderId.ToString(),
				OrderDescription = $"Заказ №{orderId}",
				Language = "RU",
				BackUrl = "https://pay.avangard.ru",
			};
		}

		public CancelPaymentRequestDTO GetCancelPaymentRequestDTO(string ticket)
		{
			return new CancelPaymentRequestDTO
			{
				Ticket = ticket,
				ShopId = _signatureSection.GetValue<long>("ShopId"),
				ShopPasswd = _signatureSection.GetValue<string>("ShopPasswd"),
			};
		}

		public SignatureParams GetSignatureParamsForRegisterOrder(int orderId, decimal orderSum)
		{
			return new SignatureParams
			{
				ShopId = _signatureSection.GetValue<long>("ShopId"),
				Sign = _signatureSection.GetValue<string>("ShopSign"),
				OrderId = orderId.ToString(),
				OrderSumInKopecks = _orderSumConverter.ConvertOrderSumToKopecks(orderSum)
			};
		}

		public SignatureParams GetSignatureParamsForValidate(PaidOrderInfoDTO paidOrderInfoDto)
		{
			return new SignatureParams
			{
				OrderId = paidOrderInfoDto.OrderNumber,
				OrderSumInKopecks = paidOrderInfoDto.Amount,
				ShopId = paidOrderInfoDto.ShopId,
				Sign = _signatureSection.GetValue<string>("AvSign")
			};
		}

		public FastPayment GetFastPayment(
			OrderRegistrationResponseDTO orderRegistrationResponseDto, Order order, DateTime creationDate, string phoneNumber = null)
		{
			return new FastPayment
			{
				Amount = order.OrderSum,
				CreationDate = creationDate,
				Order = order,
				Ticket = orderRegistrationResponseDto.Ticket,
				QRPngBase64 = orderRegistrationResponseDto.QRPngBase64,
				ExternalId = (int)orderRegistrationResponseDto.Id,
				PhoneNumber = phoneNumber
			};
		}

		public FastPayment GetFastPayment(Order order, FastPaymentDTO paymentDto)
		{
			return new FastPayment
			{
				Amount = order.OrderSum,
				CreationDate = paymentDto.CreationDate,
				Order = order,
				Ticket = paymentDto.Ticket,
				QRPngBase64 = paymentDto.QRPngBase64,
				ExternalId = paymentDto.ExternalId
			};
		}
	}
}

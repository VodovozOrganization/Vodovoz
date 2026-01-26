using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Responses;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsAPI.Library.Models
{
	public interface IFastPaymentService
	{
		FastPayment GetFastPaymentByTicket(string ticket);
		IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOrder(int orderId);
		IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(int onlineOrderId, decimal onlineOrderSum);
		void SaveNewTicketForOrder(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			int orderId,
			Guid fastPaymentGuid,
			FastPaymentPayType payType,
			Organization organization,
			FastPaymentRequestFromType fastPaymentRequestFromType,
			PaymentType paymentType,
			string phoneNumber = null);
		void SaveNewTicketForOnlineOrder(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			Guid fastPaymentGuid,
			int onlineOrderId,
			decimal onlineOrderSum,
			FastPaymentPayType payType,
			Organization organization,
			FastPaymentRequestFromType fastPaymentRequestFromType,
			string callbackUrl);
		bool UpdateFastPaymentStatus(PaidOrderInfoDTO operationInfoDto, FastPayment fastPayment);
		void UpdateFastPaymentStatus(FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate);
		bool ValidateSignature(PaidOrderInfoDTO paidOrderInfoDto, out string paymentSignature);
		Result<Organization> GetOrganization(
			TimeSpan requestTime, FastPaymentRequestFromType fastPaymentRequestFromType, Order order = null);
		/// <summary>
		/// Создание и сохранение события неверной подписи у платежа
		/// </summary>
		/// <param name="orderNumber">Номер заказа</param>
		/// <param name="bankSignature">Подпись банка</param>
		/// <param name="shopId">Идентификатор магазина</param>
		/// <param name="paymentSignature">Сгенерированная подпись</param>
		void CreateWrongFastPaymentEvent(string orderNumber, string bankSignature, int shopId, string paymentSignature);
	}
}

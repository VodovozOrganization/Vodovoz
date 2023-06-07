using System;
using System.Collections.Generic;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Responses;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsAPI.Library.Models
{
	public interface IFastPaymentModel
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
			RequestFromType requestFromType,
			PaymentType paymentType,
			string phoneNumber = null);
		void SaveNewTicketForOnlineOrder(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			Guid fastPaymentGuid,
			int onlineOrderId,
			decimal onlineOrderSum,
			FastPaymentPayType payType,
			Organization organization,
			RequestFromType requestFromType,
			string callbackUrl);
		bool UpdateFastPaymentStatus(PaidOrderInfoDTO operationInfoDto, FastPayment fastPayment);
		void UpdateFastPaymentStatus(FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate);
		bool ValidateSignature(PaidOrderInfoDTO paidOrderInfoDto, out string paymentSignature);
		Organization GetOrganization(RequestFromType requestFromType);
	}
}

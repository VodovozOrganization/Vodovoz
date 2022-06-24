using System;
using System.Collections.Generic;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Responses;
using Vodovoz.Domain.FastPayments;

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
			string phoneNumber = null);
		void SaveNewTicketForOnlineOrder(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			Guid fastPaymentGuid,
			int onlineOrderId,
			decimal onlineOrderSum,
			FastPaymentPayType payType);
		FastPayment UpdateFastPaymentStatus(PaidOrderInfoDTO operationInfoDto);
		void UpdateFastPaymentStatus(FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate);
		bool ValidateSignature(PaidOrderInfoDTO paidOrderInfoDto);
	}
}

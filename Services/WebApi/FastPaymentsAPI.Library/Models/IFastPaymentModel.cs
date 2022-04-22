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
		IList<FastPayment> GetAllPerformedOrProcessingFastPayments(int orderId);
		void SaveNewTicket(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			int orderId,
			Guid fastPaymentGuid,
			string phoneNumber = null);
		void UpdateFastPaymentStatus(PaidOrderInfoDTO operationInfoDto);
		void UpdateFastPaymentStatus(FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate);
		bool ValidateSignature(PaidOrderInfoDTO paidOrderInfoDto);
	}
}

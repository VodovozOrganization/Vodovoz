using System;
using FastPaymentsApi.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public interface IFastPaymentManager
	{
		bool IsTimeToCancelPayment(DateTime fastPaymentCreationDate, bool fastPaymentWithQRNotFromOnline, bool fastPaymentFromOnline);
		void UpdateFastPaymentStatus(IUnitOfWork uow, FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate);
	}
}

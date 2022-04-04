using System;
using FastPaymentsAPI.Library.DTO_s;
using QS.DomainModel.UoW;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers;

public interface IFastPaymentManager
{
	bool IsTimeToCancelPayment(DateTime fastPaymentCreationDate, bool fastPaymentWithQR);
	void UpdateFastPaymentStatus(IUnitOfWork uow, FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate);
}

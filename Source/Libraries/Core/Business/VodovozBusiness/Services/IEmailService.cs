using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Services
{
	public interface IEmailService
	{
		Result SendBillForClosingDocumentOrderToEmailOnFinishIfNeeded(IUnitOfWork unitOfWork, Order order);
		Result SendBillToEmail(IUnitOfWork unitOfWork, Order order);
		Result SendUpdToEmail(IUnitOfWork unitOfWork, Order order);
		Result SendUpdToEmailOnFinishIfNeeded(IUnitOfWork unitOfWork, Order order);
		Email GetEmailAddressForBill(Order order);
		OrderDocumentType[] GetRequiredDocumentTypes(Order order);
		bool NeedSendBillToEmail(IUnitOfWork unitOfWork, Order order);
		bool NeedResendBillToEmail(IUnitOfWork unitOfWork, Order order);
	}
}

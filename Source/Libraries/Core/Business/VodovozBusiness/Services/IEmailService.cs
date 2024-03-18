using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Errors;

namespace Vodovoz.Services
{
	public interface IEmailService
	{
		Result SendBillForClosingDocumentOrderToEmailOnFinishIfNeeded(IUnitOfWork uow, Order order, IEmailRepository emailRepository, IOrderRepository orderRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings);
		Result SendBillToEmail(IUnitOfWork uow, Order order);
		Result SendUpdToEmail(IUnitOfWork uow, Order order);
		Result SendUpdToEmailOnFinishIfNeeded(IUnitOfWork uow, Order order, IEmailRepository emailRepository, IDeliveryScheduleSettings deliveryScheduleSettings);
		Email GetEmailAddressForBill(Order order);
		OrderDocumentType[] GetRequirementDocTypes(Order order);
		bool NeedSendBillToEmail(IUnitOfWork uow, Order order, IOrderRepository orderRepository, IEmailRepository emailRepository);
	}
}

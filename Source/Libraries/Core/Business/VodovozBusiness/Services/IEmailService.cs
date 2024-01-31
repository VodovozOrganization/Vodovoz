using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Services
{
	public interface IEmailService
	{
		void SendBillForClosingDocumentOrderToEmailOnFinish(IUnitOfWork uow, Order order, IEmailRepository emailRepository, IOrderRepository orderRepository,
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider);
		void SendBillToEmail(IUnitOfWork uow, Order order, IEmailRepository emailRepository);
		void SendUpdToEmail(IUnitOfWork uow, Order order);
		void SendUpdToEmailOnFinish(IUnitOfWork uow, Order order, IEmailRepository emailRepository, IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider);
		Email GetEmailAddressForBill(Order order);
		OrderDocumentType[] GetRequirementDocTypes(Order order);
		bool NeedSendBillToEmail(IUnitOfWork uow, Order order, IOrderRepository orderRepository, IEmailRepository emailRepository);
	}
}

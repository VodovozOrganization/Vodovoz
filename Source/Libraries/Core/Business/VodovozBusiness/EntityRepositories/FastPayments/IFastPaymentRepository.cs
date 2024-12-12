using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.EntityRepositories.FastPayments
{
	public interface IFastPaymentRepository
	{
		IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOrder(IUnitOfWork uow, int orderId);
		IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(IUnitOfWork uow, int onlineOrderId, decimal onlineOrderSum);
		FastPaymentStatus? GetOrderFastPaymentStatus(IUnitOfWork uow, int orderId, int? onlineOrder = null);
		FastPayment GetFastPaymentByTicket(IUnitOfWork uow, string ticket);
		FastPayment GetFastPaymentByGuid(IUnitOfWork uow, Guid fastPaymentGuid);
		bool FastPaymentWithTicketExists(IUnitOfWork uow, string ticket);
		IEnumerable<FastPayment> GetAllProcessingFastPayments(IUnitOfWork uow);
		FastPayment GetProcessingPaymentForOrder(IUnitOfWork uow, int orderId);
		FastPayment GetPerformedFastPaymentByExternalId(IUnitOfWork uow, int externalId);
		IList<FastPayment> GetAllPaymentsByOnlineOrder(IUnitOfWork uow, int orderId);
		FastPaymentNotification GetNotificationsForPayment(IUnitOfWork uow, FastPaymentNotificationType notificationType, int orderId);
		IEnumerable<FastPaymentNotification> GetActiveNotifications(IUnitOfWork uow);
	}
}

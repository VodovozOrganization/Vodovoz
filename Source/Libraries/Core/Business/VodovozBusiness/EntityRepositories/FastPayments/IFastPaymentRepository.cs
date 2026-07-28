using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.EntityRepositories.FastPayments
{
	public interface IFastPaymentRepository
	{
		/// <summary>
		/// Получение всех быстрых платежей по идентификатору заказа, которые находятся в статусе "Исполнен" или "Обрабатывается"
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список быстрых платежей</returns>
		IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получение всех быстрых платежей по идентификатору онлайн-заказа, которые находятся в статусе "Исполнен" или "Обрабатывается"
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="onlineOrderId">Идентификатор онлайн-заказа</param>
		/// <param name="onlineOrderSum">Сумма онлайн-заказа</param>
		/// <returns>Список быстрых платежей</returns>

		IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(IUnitOfWork uow, int onlineOrderId, decimal onlineOrderSum);

		/// <summary>
		/// Получение статуса быстрого платежа по идентификатору заказа и онлайн-заказа
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="onlineOrder">Идентификатор онлайн-заказа</param>
		/// <returns>Статус быстрого платежа</returns>
		FastPaymentStatus? GetOrderFastPaymentStatus(IUnitOfWork uow, int orderId, int? onlineOrder = null);

		/// <summary>
		/// Получение быстрого платежа по тикету
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="ticket">Тикет</param>
		/// <returns>Быстрый платеж</returns>
		FastPayment GetFastPaymentByTicket(IUnitOfWork uow, string ticket);

		/// <summary>
		/// Получение быстрого платежа по Guid
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="fastPaymentGuid">Guid быстрого платежа</param>
		/// <returns>Быстрый платеж</returns>
		FastPayment GetFastPaymentByGuid(IUnitOfWork uow, Guid fastPaymentGuid);

		/// <summary>
		/// Проверка существования быстрого платежа по тикету
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="ticket">Тикет</param>
		/// <returns>True, если платеж существует, иначе False</returns>
		bool FastPaymentWithTicketExists(IUnitOfWork uow, string ticket);

		/// <summary>
		/// Получение всех быстрых платежей, которые находятся в статусе "Обрабатывается"
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <returns>Коллекция быстрых платежей</returns>
		IEnumerable<FastPayment> GetAllProcessingFastPayments(IUnitOfWork uow);

		/// <summary>
		/// Получение быстрого платежа, который находится в статусе "Обрабатывается" по идентификатору заказа
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Быстрый платеж</returns>
		FastPayment GetProcessingPaymentForOrder(IUnitOfWork uow, int orderId);
		/// <summary>
		/// Получение успешного(оплаченного) быстрого платежа по номеру и дате создания(опционально)
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="externalId">Номер оплаты</param>
		/// <param name="creationDate">Дата создания платежа</param>
		/// <returns>Быстрый платеж</returns>
		FastPayment GetPerformedFastPaymentByExternalId(IUnitOfWork uow, int externalId, DateTime? creationDate = null);

		/// <summary>
		/// Получить быстрого платежа по номеру и дате создания(опционально)
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="externalId">Номер оплаты</param>
		/// <param name="creationDate">Дата создания платежа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Быстрый платеж</returns>
		Task<FastPayment> GetFastPaymentByExternalIdAsync(
			IUnitOfWork uow,
			int externalId,
			DateTime? creationDate = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Получить все быстрые платежи по онлайн-заказу
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="orderId">ID онлайн-заказа</param>
		/// <returns>Список быстрых платежей</returns>
		IList<FastPayment> GetAllPaymentsByOnlineOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получить уведомление о платеже по типу и ID заказа
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="notificationType">Тип уведомления</param>
		/// <param name="orderId">ID заказа</param>
		/// <returns>Уведомление о платеже</returns>
		FastPaymentNotification GetNotificationsForPayment(IUnitOfWork uow, FastPaymentNotificationType notificationType, int orderId);

		/// <summary>
		/// Получить все активные уведомления о платеже
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <returns>Коллекция активных уведомлений</returns>
		IEnumerable<FastPaymentNotification> GetActiveNotifications(IUnitOfWork uow);
	}
}

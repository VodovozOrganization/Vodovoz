using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sms;

namespace Vodovoz.EntityRepositories.SmsNotifications
{
	public interface ISmsNotificationRepository
	{
		/// <summary>
		/// Получить уведомления клиенту независимо от статуса.
		/// </summary>
		/// <param name="uow">Единица работы.</param>
		/// <param name="counterpartyId">Идентификатор контрагента.</param>
		/// <returns>Уведомления контрагента.</returns>
		IEnumerable<NewClientSmsNotification> GetNewClientSmsNotifications(IUnitOfWork uow, int counterpartyId);

		IEnumerable<NewClientSmsNotification> GetUnsendedNewClientSmsNotifications(IUnitOfWork uow);
		IEnumerable<UndeliveryNotApprovedSmsNotification> GetUnsendedUndeliveryNotApprovedSmsNotifications(IUnitOfWork uow);
		IEnumerable<CourierOnTheWaySmsNotification> GetUnsendedCourierOnTheWaySmsNotifications(IUnitOfWork uow);

		/// <summary>
		/// Создавалось ли ранее смс уведомление о том, что курьер в пути, по указанному заказу
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="driverId">Идентификатор водителя</param>
		bool HasCourierOnTheWaySmsNotification(IUnitOfWork uow, int orderId, int driverId);
	}
}

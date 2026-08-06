using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sms;

namespace Vodovoz.EntityRepositories.SmsNotifications
{
	public interface ISmsNotificationRepository
	{
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

using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IExternalCounterpartyAssignNotificationRepository
	{
		IList<ExternalCounterpartyAssignNotification> GetNotificationsForSend(IUnitOfWork uow, int days);

		/// <summary>Получить все уведомления для указанных пользователей ИПЗ.</summary>
		/// <param name="uow">Текущая единица работы.</param>
		/// <param name="externalCounterpartyIds">Идентификаторы пользователей ИПЗ.</param>
		/// <returns>Уведомления независимо от даты и результата отправки.</returns>
		IList<ExternalCounterpartyAssignNotification> GetByExternalCounterpartyIds(
			IUnitOfWork uow, IEnumerable<int> externalCounterpartyIds);
	}
}

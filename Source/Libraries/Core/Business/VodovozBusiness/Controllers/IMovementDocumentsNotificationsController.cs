using QS.DomainModel.UoW;
using System;

namespace Vodovoz.Controllers
{
	public interface IMovementDocumentsNotificationsController
	{
		SentMovementsNotificationDetails GetNotificationDetails(IUnitOfWork uow);
		(bool Alert, string Message) GetNotificationMessage(IUnitOfWork uow);
		event Action<(bool Alert, string Message)> UpdateNotificationAction;
	}
}

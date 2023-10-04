using QS.DomainModel.UoW;
using System;

namespace Vodovoz.Controllers
{
	public interface IMovementDocumentsNotificationsController
	{
		SendedMovementsNotificationDetails GetNotificationDetails(IUnitOfWork uow);
		string GetNotificationMessage(IUnitOfWork uow);
		event Action<SendedMovementsNotificationDetails> UpdateNotificationAction;
	}
}

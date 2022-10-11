using QS.DomainModel.UoW;
using System;

namespace Vodovoz.Controllers
{
	public interface IMovementDocumentsNotificationsController
	{
		SendedMovementsNotificationDetails GetNotificationDetails(IUnitOfWork uow);
		string GetNotificationMessageBySubdivision(IUnitOfWork uow);
		event Action<string> UpdateNotificationAction;
	}
}

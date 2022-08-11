using QS.DomainModel.UoW;

namespace Vodovoz.Controllers
{
	public interface IMovementDocumentsNotificationsController
	{
		SendedMovementsNotificationDetails GetNotificationDetails(IUnitOfWork uow, Subdivision subdivision);
		string GetNotificationMessageBySubdivision(IUnitOfWork uow, int subdivisionId);
	}
}

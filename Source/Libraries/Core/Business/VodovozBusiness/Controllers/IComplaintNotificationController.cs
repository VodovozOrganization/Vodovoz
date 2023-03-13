using QS.DomainModel.UoW;
using System;

namespace Vodovoz.Controllers
{
	public interface IComplaintNotificationController
	{
		SendedComplaintNotificationDetails GetNotificationDetails(IUnitOfWork uow);
		string GetNotificationMessageBySubdivision(IUnitOfWork uow);
		event Action<string> UpdateNotificationAction;
	}
}

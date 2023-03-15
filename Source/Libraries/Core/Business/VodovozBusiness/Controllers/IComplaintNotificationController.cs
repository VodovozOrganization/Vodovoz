using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;

namespace Vodovoz.Controllers
{
	public interface IComplaintNotificationController
	{
		SendedComplaintNotificationDetails GetNotificationDetails(IUnitOfWork uow);
		List<int> GetSendedComplaintIdsBySubdivision(IUnitOfWork uow);
		string GetNotificationMessageBySubdivision(IUnitOfWork uow);
		event Action<string> UpdateNotificationAction;
	}
}

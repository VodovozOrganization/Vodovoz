using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;

namespace Vodovoz.Controllers
{
	public interface IComplaintNotificationController
	{
		SendedComplaintNotificationDetails GetNotificationDetails(IUnitOfWork uow);
		event Action<SendedComplaintNotificationDetails> UpdateNotificationAction;
	}
}

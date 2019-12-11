using System;
using System.Collections.Generic;
using Vodovoz.Domain.Sms;
using QS.DomainModel.UoW;
namespace Vodovoz.EntityRepositories.SmsNotifications
{
	public interface ISmsNotificationRepository
	{
		IEnumerable<NewClientSmsNotification> GetUnsendedNewClientSmsNotifications(IUnitOfWork uow);
	}
}

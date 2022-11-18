using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sms;

namespace Vodovoz.EntityRepositories.SmsNotifications
{
	public interface ISmsNotificationRepository
	{
		IEnumerable<NewClientSmsNotification> GetUnsendedNewClientSmsNotifications(IUnitOfWork uow);
		IEnumerable<UndeliveryNotApprovedSmsNotification> GetUnsendedUndeliveryNotApprovedSmsNotifications(IUnitOfWork uow);
	}
}

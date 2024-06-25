using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sms;

namespace Vodovoz.EntityRepositories.SmsNotifications
{
	public class SmsNotificationRepository : ISmsNotificationRepository
	{
		public IEnumerable<NewClientSmsNotification> GetUnsendedNewClientSmsNotifications(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<NewClientSmsNotification>()
						.Where(x => x.Status == SmsNotificationStatus.New)
						.List();
		}

		public IEnumerable<UndeliveryNotApprovedSmsNotification> GetUnsendedUndeliveryNotApprovedSmsNotifications(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<UndeliveryNotApprovedSmsNotification>()
				.Where(x => x.Status == SmsNotificationStatus.New)
				.List();
		}
	}
}

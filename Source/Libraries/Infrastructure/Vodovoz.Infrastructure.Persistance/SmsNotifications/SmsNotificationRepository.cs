using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.SmsNotifications;

namespace Vodovoz.Infrastructure.Persistance.SmsNotifications
{
	internal sealed class SmsNotificationRepository : ISmsNotificationRepository
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

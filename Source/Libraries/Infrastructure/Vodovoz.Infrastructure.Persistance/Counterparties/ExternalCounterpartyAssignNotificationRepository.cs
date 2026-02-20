using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class ExternalCounterpartyAssignNotificationRepository : IExternalCounterpartyAssignNotificationRepository
	{
		public IList<ExternalCounterpartyAssignNotification> GetNotificationsForSend(IUnitOfWork uow, int days)
		{
			return uow.Session.QueryOver<ExternalCounterpartyAssignNotification>()
				.Where(n => n.HttpCode == null || (n.HttpCode != 204 && n.HttpCode != 405))
				.And(n => n.CreationDate >= DateTime.Today.AddDays(-days))
				.List();
		}
	}
}

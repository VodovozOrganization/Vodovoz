using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.Repositories
{
	public static class EmailRepository
	{
		public static List<StoredEmail> GetAllEmailsForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<StoredEmail>()
				      .Where(x => x.Order.Id == orderId)
				      .List()
				      .ToList();
		}

		public static StoredEmail GetStoredEmailByMessageId(IUnitOfWork uow, string messageId)
		{
			return uow.Session.QueryOver<StoredEmail>().Where(x => x.ExternalId == messageId).SingleOrDefault();
		}

		public static bool HaveSendedEmail(int orderId, OrderDocumentType type)
		{
			IList<StoredEmail> result;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ES]Получение списка отправленных писем")){

				result = uow.Session.QueryOver<StoredEmail>()
				            .Where(x => x.Order.Id == orderId)
				            .Where(x => x.DocumentType == type)
				            .Where(x => x.State != StoredEmailStates.SendingError
				                   && x.State != StoredEmailStates.Undelivered)
				            .List();
			}
			return result.Any();
		}

		public static bool CanSendByTimeout(string address, int orderId)
		{
			// Время в минутах, по истечению которых будет возможна повторная отправка
			double timeLimit = 10;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ES]Получение возможна ли повторная отправка")) {
				var lastSendTime = uow.Session.QueryOver<StoredEmail>()
									  .Where(x => x.RecipientAddress == address)
									  .Where(x => x.Order.Id == orderId)
									  .Where(x => x.State != StoredEmailStates.SendingError)
									  .Select(Projections.Max<StoredEmail>(y => y.SendDate))
									  .SingleOrDefault<DateTime>();
				if(lastSendTime != default(DateTime)) {
					return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
				}
			}
			return true;
		}
	}
}

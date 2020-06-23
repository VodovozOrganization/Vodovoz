using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.EntityRepositories
{
	public class EmailRepository : IEmailRepository
	{
		public List<StoredEmail> GetAllEmailsForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<StoredEmail>()
				      .Where(x => x.Order.Id == orderId)
				      .List()
				      .ToList();
		}

		public StoredEmail GetStoredEmailByMessageId(IUnitOfWork uow, string messageId)
		{
			return uow.Session.QueryOver<StoredEmail>().Where(x => x.ExternalId == messageId).SingleOrDefault();
		}

		public bool HaveSendedEmail(int orderId, OrderDocumentType type)
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

		public bool CanSendByTimeout(string address, int orderId, OrderDocumentType type)
		{
			// Время в минутах, по истечению которых будет возможна повторная отправка
			double timeLimit = 10;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ES]Получение возможна ли повторная отправка")) {
				if(type == OrderDocumentType.Bill) {
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
				else if(type == OrderDocumentType.BillWithoutShipmentForDebt) {
					var lastSendTime = uow.Session.QueryOver<StoredEmail>()
										  .Where(x => x.RecipientAddress == address)
										  .Where(x => x.OrderWithoutShipmentForDebt.Id == orderId)
										  .Where(x => x.State != StoredEmailStates.SendingError)
										  .Select(Projections.Max<StoredEmail>(y => y.SendDate))
										  .SingleOrDefault<DateTime>();
					if(lastSendTime != default(DateTime)) {
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
				else if(type == OrderDocumentType.BillWithoutShipmentForAdvancePayment) {
					var lastSendTime = uow.Session.QueryOver<StoredEmail>()
										  .Where(x => x.RecipientAddress == address)
										  .Where(x => x.OrderWithoutShipmentForAdvancePayment.Id == orderId)
										  .Where(x => x.State != StoredEmailStates.SendingError)
										  .Select(Projections.Max<StoredEmail>(y => y.SendDate))
										  .SingleOrDefault<DateTime>();
					if(lastSendTime != default(DateTime)) {
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
				else if(type == OrderDocumentType.BillWithoutShipmentForPayment) {
					var lastSendTime = uow.Session.QueryOver<StoredEmail>()
										  .Where(x => x.RecipientAddress == address)
										  .Where(x => x.OrderWithoutShipmentForPayment.Id == orderId)
										  .Where(x => x.State != StoredEmailStates.SendingError)
										  .Select(Projections.Max<StoredEmail>(y => y.SendDate))
										  .SingleOrDefault<DateTime>();
					if(lastSendTime != default(DateTime)) {
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
			}
			return true;
		}

		#region EmailType

		public IList<EmailType> GetEmailTypes(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<EmailType>().List<EmailType>();
		}

		public EmailType EmailTypeWithPurposeExists(IUnitOfWork uow, EmailPurpose emailPurpose)
		{
			return uow.Session.QueryOver<EmailType>()
				.Where(x => x.EmailPurpose == emailPurpose).Take(1)
				.SingleOrDefault<EmailType>();
		}

		#endregion
	}
}

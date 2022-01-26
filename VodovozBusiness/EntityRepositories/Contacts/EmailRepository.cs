using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories
{
	public class EmailRepository : IEmailRepository
	{
		public StoredEmail GetById(IUnitOfWork unitOfWork, int id)
		{
			return unitOfWork.GetById<StoredEmail>(id);
		}

		public List<StoredEmail> GetAllEmailsForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<OrderDocumentEmail>()
					.Where(ode => ode.Order.Id == orderId)
					.Select(ode => ode.StoredEmail)
					.List<StoredEmail>().ToList();
		}

		public List<OrderDocumentEmail> GetEmailsForPreparingOrderDocuments(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<OrderDocumentEmail>()
				.JoinQueryOver(ode => ode.StoredEmail)
				.Where(se => se.State == StoredEmailStates.PreparingToSend)
				.List()
				.ToList();
		}

		public List<BillDocument> GetAllUnsentDocuments(IUnitOfWork uow, DateTime date)
		{
			VodOrder orderAlias = null;

			return uow.Session.QueryOver<BillDocument>()
					  .Left.JoinAlias(bdoc => bdoc.Order, () => orderAlias)
					  .Where(() => orderAlias.CreateDate >= date)
					  .WithSubquery.WhereNotExists(
					  	QueryOver.Of<OrderDocumentEmail>()
						.Where(ode => ode.Order.Id == orderAlias.Id)
						.Select(x => x.Id))
					  .List().Take(1)
					  .ToList();
		}

		public StoredEmail GetStoredEmailByMessageId(IUnitOfWork uow, string messageId)
		{
			return uow.Session.QueryOver<StoredEmail>().Where(x => x.ExternalId == messageId).SingleOrDefault();
		}

		public bool HaveSendedEmailForBill(int orderId)
		{
			IList<OrderDocumentEmail> result;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ES]Получение списка отправленных писем"))
			{
				OrderDocumentEmail orderDocumentEmailAlias = null;
				result = uow.Session.QueryOver<OrderDocumentEmail>(()=>orderDocumentEmailAlias)
					.Where(od => od.Order.Id == orderId)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(se=> se.State != StoredEmailStates.SendingError 
					           && se.State != StoredEmailStates.Undelivered)
					.WithSubquery.WhereExists(
						QueryOver.Of<BillDocument>()
							.Where(bd => bd.Id == orderDocumentEmailAlias.OrderDocument.Id)
							.Select(bd => bd.Id))
					.List();
			}

			return result.Any();
		}

		public bool CanSendByTimeout(string address, int orderId, OrderDocumentType type)
		{
			// Время в минутах, по истечению которых будет возможна повторная отправка
			double timeLimit = 10;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[ES]Получение возможна ли повторная отправка"))
			{
				if(type == OrderDocumentType.Bill)
				{
					StoredEmail storedEmailAlias = null;
					var lastSendTime = uow.Session.QueryOver<OrderDocumentEmail>()
						.Where(ode => ode.Order.Id == orderId)
						.JoinAlias(ode=> ode.StoredEmail, () => storedEmailAlias)
						.Where(() => storedEmailAlias.RecipientAddress == address)
						.And(() => storedEmailAlias.State != StoredEmailStates.SendingError)
						.Select(Projections.Max(() => storedEmailAlias.SendDate))
						.SingleOrDefault<DateTime>();
					if(lastSendTime != default(DateTime))
					{
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
				else if(type == OrderDocumentType.BillWSForDebt) {
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
				else if(type == OrderDocumentType.BillWSForAdvancePayment) {
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
				else if(type == OrderDocumentType.BillWSForPayment) {
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

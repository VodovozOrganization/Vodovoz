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
				.JoinQueryOver(ode => ode.OrderDocument)
					.Where(od => od.Order.Id == orderId)
					.Select(ode => ode.StoredEmail)
					.List<StoredEmail>().ToList();
		}

		public List<CounterpartyEmail> GetEmailsForPreparingOrderDocuments(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<CounterpartyEmail>()
				.JoinQueryOver(ode => ode.StoredEmail)
				.Where(se => se.State == StoredEmailStates.PreparingToSend)
				.List()
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
				OrderDocument orderDocumentAlias = null;

				result = uow.Session.QueryOver<OrderDocumentEmail>(() => orderDocumentEmailAlias)
					.JoinAlias(() => orderDocumentEmailAlias.OrderDocument, () => orderDocumentAlias)
					.Where(() => orderDocumentAlias.Order.Id == orderId)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(se => se.State != StoredEmailStates.SendingError 
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
				if(type == OrderDocumentType.Bill || type == OrderDocumentType.SpecialBill)
				{
					StoredEmail storedEmailAlias = null;
					OrderDocument orderDocumentAlias = null;
					var lastSendTime = uow.Session.QueryOver<OrderDocumentEmail>()
						.JoinAlias(ode => ode.OrderDocument, () => orderDocumentAlias)
						.Where(() => orderDocumentAlias.Order.Id == orderId)
						.JoinAlias(ode => ode.StoredEmail, () => storedEmailAlias)
						.Where(() => storedEmailAlias.RecipientAddress == address)
						.And(() => storedEmailAlias.State != StoredEmailStates.SendingError)
						.Select(Projections.Max(() => storedEmailAlias.SendDate))
						.SingleOrDefault<DateTime>();

					if(lastSendTime != default(DateTime))
					{
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
				else if(type == OrderDocumentType.BillWSForDebt)
				{
					StoredEmail storedEmailAlias = null;
					var lastSendTime = uow.Session.QueryOver<OrderWithoutShipmentForDebtEmail>()
						.Where(ode => ode.OrderWithoutShipmentForDebt.Id == orderId)
						.JoinAlias(ode => ode.StoredEmail, () => storedEmailAlias)
						.Where(() => storedEmailAlias.RecipientAddress == address)
						.And(() => storedEmailAlias.State != StoredEmailStates.SendingError)
						.Select(Projections.Max(() => storedEmailAlias.SendDate))
						.SingleOrDefault<DateTime>();

					if(lastSendTime != default(DateTime))
					{
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
				else if(type == OrderDocumentType.BillWSForAdvancePayment)
				{
					StoredEmail storedEmailAlias = null;
					var lastSendTime = uow.Session.QueryOver<OrderWithoutShipmentForAdvancePaymentEmail>()
						.Where(ode => ode.OrderWithoutShipmentForAdvancePayment.Id == orderId)
						.JoinAlias(ode => ode.StoredEmail, () => storedEmailAlias)
						.Where(() => storedEmailAlias.RecipientAddress == address)
						.And(() => storedEmailAlias.State != StoredEmailStates.SendingError)
						.Select(Projections.Max(() => storedEmailAlias.SendDate))
						.SingleOrDefault<DateTime>();

					if(lastSendTime != default(DateTime))
					{
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
				else if(type == OrderDocumentType.BillWSForPayment)
				{
					StoredEmail storedEmailAlias = null;
					var lastSendTime = uow.Session.QueryOver<OrderWithoutShipmentForPaymentEmail>()
						.Where(ode => ode.OrderWithoutShipmentForPayment.Id == orderId)
						.JoinAlias(ode => ode.StoredEmail, () => storedEmailAlias)
						.Where(() => storedEmailAlias.RecipientAddress == address)
						.And(() => storedEmailAlias.State != StoredEmailStates.SendingError)
						.Select(Projections.Max(() => storedEmailAlias.SendDate))
						.SingleOrDefault<DateTime>();

					if(lastSendTime != default(DateTime))
					{
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
			}
			return true;
		}

		public int GetCurrentDatabaseId(IUnitOfWork uow)
		{
			return Convert.ToInt32(uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());
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

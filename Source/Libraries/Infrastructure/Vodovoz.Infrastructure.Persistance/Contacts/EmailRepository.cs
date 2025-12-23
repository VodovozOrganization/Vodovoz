using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Contacts
{
	internal sealed class EmailRepository : IEmailRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public EmailRepository(
			IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public StoredEmail GetById(IUnitOfWork unitOfWork, int id)
		{
			return unitOfWork.GetById<StoredEmail>(id);
		}

		public List<StoredEmail> GetAllEmailsForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<BillDocumentEmail>()
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
			using(var uow = _uowFactory.CreateWithoutRoot($"[ES]Получение списка отправленных писем"))
			{
				var result =
					(
						from billEmail in uow.Session.Query<BillDocumentEmail>()
						where !new[] { StoredEmailStates.SendingError, StoredEmailStates.Undelivered }.Contains(billEmail.StoredEmail.State)
							&& billEmail.OrderDocument.Order.Id == orderId

						let billDocument = from billDoc in uow.Session.Query<BillDocument>()
										   where billDoc.Id == billEmail.OrderDocument.Id
										   select billDoc

						let specBillDocument = from specBillDoc in uow.Session.Query<SpecialBillDocument>()
											   where specBillDoc.Id == billEmail.OrderDocument.Id
											   select specBillDoc

						where billDocument != null || specBillDocument != null

						select billEmail.Id

					)
					.Count() > 0;

				return result;
			}
		}

		public bool HasSendedEmailsForBillExceptOf(int orderId, int emailId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot($"[ES]Получение списка отправленных писем"))
			{
				var result =
					(
						from billEmail in uow.Session.Query<BillDocumentEmail>()
						where !new[] { StoredEmailStates.SendingError, StoredEmailStates.Undelivered }.Contains(billEmail.StoredEmail.State)
							&& billEmail.OrderDocument.Order.Id == orderId

						let billDocument = from billDoc in uow.Session.Query<BillDocument>()
										   where billDoc.Id == billEmail.OrderDocument.Id
										   select billDoc

						let specBillDocument = from specBillDoc in uow.Session.Query<SpecialBillDocument>()
											   where specBillDoc.Id == billEmail.OrderDocument.Id
											   select specBillDoc

						where (billDocument != null || specBillDocument != null)
							&& billEmail.StoredEmail.Id != emailId
						select billEmail.Id
					)
					.Count() > 0;

				return result;
			}
		}

		public bool HasSendedEmailForUpd(int orderId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot($"Получение списка отправленных писем c УПД"))
			{
				return (from documentEmail in uow.GetAll<UpdDocumentEmail>()
						where documentEmail.OrderDocument.Order.Id == orderId
							  && !new[] { StoredEmailStates.SendingError, StoredEmailStates.Undelivered }.Contains(documentEmail.StoredEmail.State)
						let upd = uow.GetAll<UPDDocument>()
							.Where(ud => ud.Id == documentEmail.OrderDocument.Id)
						let specUpd = uow.GetAll<SpecialUPDDocument>()
							.Where(ud => ud.Id == documentEmail.OrderDocument.Id)
						where upd.Any() || specUpd.Any()
						select documentEmail.Id)
					.Any();
			}
		}

		public bool NeedSendDocumentsByEmailOnFinish(
			IUnitOfWork uow,
			Order currentOrder,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			bool isForBill = false)
		{
			if(currentOrder.PaymentType != PaymentType.Cashless)
			{
				return false;
			}

			var result = (
				from order in uow.Session.Query<Order>()
				join contract in uow.Session.Query<CounterpartyContract>()
					on order.Contract.Id equals contract.Id
				join organization in uow.Session.Query<Organization>()
					on contract.Organization.Id equals organization.Id
				join address in uow.Session.Query<RouteListItem>()
					on order.Id equals address.Order.Id into addresses
				from address in addresses.DefaultIfEmpty()
				join defaultEdoAccount in uow.Session.Query<CounterpartyEdoAccountEntity>()
					on new { a = order.Client.Id, b = (int?)contract.Organization.Id, c = true }
					equals new { a = defaultEdoAccount.Counterparty.Id, b = defaultEdoAccount.OrganizationId, c = defaultEdoAccount.IsDefault }
					into edoAccountsByOrder
				from edoAccountByOrder in edoAccountsByOrder.DefaultIfEmpty()
				where
					order.Id == currentOrder.Id
					&&
					(
						order.IsFastDelivery
						||
						address.Status != RouteListItemStatus.Transfered
							&& address.AddressTransferType == AddressTransferType.FromFreeBalance
						||
						(
							(
								(isForBill && !order.Client.NeedSendBillByEdo)
								|| edoAccountByOrder.ConsentForEdoStatus != ConsentForEdoStatus.Agree
							)
							&& order.DeliverySchedule.Id == deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId
						)
					)
				select order.Id)
			.Any();

			return result;
		}

		public bool CanSendByTimeout(string address, int orderId, OrderDocumentType type)
		{
			// Время в минутах, по истечению которых будет возможна повторная отправка
			double timeLimit = 10;
			using(var uow = _uowFactory.CreateWithoutRoot($"[ES]Получение возможна ли повторная отправка"))
			{
				if(type == OrderDocumentType.Bill || type == OrderDocumentType.SpecialBill)
				{
					StoredEmail storedEmailAlias = null;
					OrderDocument orderDocumentAlias = null;
					var lastSendTime = uow.Session.QueryOver<BillDocumentEmail>()
						.JoinAlias(ode => ode.OrderDocument, () => orderDocumentAlias)
						.Where(() => orderDocumentAlias.Order.Id == orderId)
						.JoinAlias(ode => ode.StoredEmail, () => storedEmailAlias)
						.Where(() => storedEmailAlias.RecipientAddress == address)
						.And(() => storedEmailAlias.State != StoredEmailStates.SendingError)
						.Select(Projections.Max(() => storedEmailAlias.SendDate))
						.SingleOrDefault<DateTime>();

					if(lastSendTime != default)
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

					if(lastSendTime != default)
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

					if(lastSendTime != default)
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

					if(lastSendTime != default)
					{
						return DateTime.Now.Subtract(lastSendTime).TotalMinutes > timeLimit;
					}
				}
				else if(type == OrderDocumentType.UPD || type == OrderDocumentType.SpecialUPD)
				{
					StoredEmail storedEmailAlias = null;
					OrderDocument orderDocumentAlias = null;
					var lastSendTime = uow.Session.QueryOver<UpdDocumentEmail>()
						.JoinAlias(ude => ude.OrderDocument, () => orderDocumentAlias)
						.Where(() => orderDocumentAlias.Order.Id == orderId)
						.JoinAlias(ode => ode.StoredEmail, () => storedEmailAlias)
						.Where(() => storedEmailAlias.RecipientAddress == address)
						.And(() => storedEmailAlias.State != StoredEmailStates.SendingError)
						.Select(Projections.Max(() => storedEmailAlias.SendDate))
						.SingleOrDefault<DateTime>();

					if(lastSendTime != default)
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

		public int GetCounterpartyIdByEmailGuidForUnsubscribing(IUnitOfWork uow, Guid emailGuid)
		{
			BulkEmailEvent bulkEmailEventAlias = null;
			CounterpartyEmail counterpartyEmailAlias = null;
			StoredEmail storedEmailAlias = null;
			GuidCounterpartyEmailNode resultAlias = null;

			var guidCounterpartyEmail = uow.Session.QueryOver(() => counterpartyEmailAlias)
				.JoinEntityAlias(() => bulkEmailEventAlias, () => counterpartyEmailAlias.Counterparty.Id == bulkEmailEventAlias.Counterparty.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => counterpartyEmailAlias.StoredEmail, () => storedEmailAlias)
				.Where(() => storedEmailAlias.Guid == emailGuid)
				.OrderBy(() => bulkEmailEventAlias.ActionTime).Desc
				.SelectList(list => list
					.Select(() => counterpartyEmailAlias.Counterparty.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => bulkEmailEventAlias.Type).WithAlias(() => resultAlias.BulkEmailEventType))
				.TransformUsing(Transformers.AliasToBean<GuidCounterpartyEmailNode>())
				.Take(1)
				.List<GuidCounterpartyEmailNode>()
				.SingleOrDefault();

			return guidCounterpartyEmail == null || guidCounterpartyEmail.BulkEmailEventType == BulkEmailEvent.BulkEmailEventType.Unsubscribing
			? 0
			: guidCounterpartyEmail.CounterpartyId;
		}

		public BulkEmailEvent GetLastBulkEmailEvent(IUnitOfWork uow, int counterpartyId)
		{
			BulkEmailEvent bulkEmailEventAlias = null;

			return uow.Session.QueryOver(() => bulkEmailEventAlias)
				.Where(() => bulkEmailEventAlias.Counterparty.Id == counterpartyId)
				.OrderBy(() => bulkEmailEventAlias.ActionTime).Desc
				.Take(1)
				.SingleOrDefault();
		}

		public BulkEmailEventReason GetBulkEmailEventOtherReason(IUnitOfWork uow, IEmailSettings emailSettings)
		{
			return uow.GetById<BulkEmailEventReason>(emailSettings.BulkEmailEventOtherReasonId);
		}

		public BulkEmailEventReason GetBulkEmailEventOperatorReason(IUnitOfWork uow, IEmailSettings emailSettings)
		{
			return uow.GetById<BulkEmailEventReason>(emailSettings.BulkEmailEventOperatorReasonId);
		}

		public Email GetEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			Email email = null;

			IList<Func<IUnitOfWork, int, Email>> queries = new List<Func<IUnitOfWork, int, Email>>
			{
				GetBillsEmailForExternalCounterparty,
				GetWorkEmailForExternalCounterparty,
				GetPersonalEmailForExternalCounterparty,
				GetReceiptsEmailForExternalCounterparty,
				GetEmailWithoutTypeForExternalCounterparty
			};

			for(var i = 0; i < queries.Count; i++)
			{
				email = queries[i].Invoke(uow, counterpartyId);

				if(email != null)
				{
					return email;
				}
			}

			return email;
		}

		private Email GetBillsEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			return GetEmailByTypeForExternalCounterparty(counterpartyId, EmailPurpose.ForBills)
				.GetExecutableQueryOver(uow.Session)
				.SingleOrDefault();
		}

		private Email GetWorkEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			return GetEmailByTypeForExternalCounterparty(counterpartyId, EmailPurpose.Work)
				.GetExecutableQueryOver(uow.Session)
				.SingleOrDefault();
		}

		private Email GetPersonalEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			return GetEmailByTypeForExternalCounterparty(counterpartyId, EmailPurpose.Personal)
				.GetExecutableQueryOver(uow.Session)
				.SingleOrDefault();
		}

		private Email GetReceiptsEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			return GetEmailByTypeForExternalCounterparty(counterpartyId, EmailPurpose.ForReceipts)
				.GetExecutableQueryOver(uow.Session)
				.SingleOrDefault();
		}

		private QueryOver<Email> GetEmailByTypeForExternalCounterparty(int counterpartyId, EmailPurpose emailPurpose)
		{
			EmailType emailTypeAlias = null;

			return QueryOver.Of<Email>()
				.JoinAlias(e => e.EmailType, () => emailTypeAlias)
				.Where(e => e.Counterparty.Id == counterpartyId)
				.And(() => emailTypeAlias.EmailPurpose == emailPurpose)
				.Take(1);
		}

		private Email GetEmailWithoutTypeForExternalCounterparty(IUnitOfWork uow, int counterpartyId)
		{
			return GetEmailWithoutTypeForExternalCounterparty(counterpartyId)
				.GetExecutableQueryOver(uow.Session)
				.SingleOrDefault();
		}

		private QueryOver<Email> GetEmailWithoutTypeForExternalCounterparty(int counterpartyId)
		{
			EmailType emailTypeAlias = null;

			return QueryOver.Of<Email>()
				.Left.JoinAlias(e => e.EmailType, () => emailTypeAlias)
				.Where(e => e.Counterparty.Id == counterpartyId)
				.And(() => emailTypeAlias.Id == null)
				.Take(1);
		}

		public Task<int> GetPendingEmailsCountAsync(IUnitOfWork uow, DateTime cutoffDate)
		{
			throw new NotImplementedException();
		}

		public Task MarkOrdersAsSentAsync(IUnitOfWork uow, List<int> orderIds, DateTime sentDate, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<Dictionary<Order, (Counterparty, Organization)>> GetAllOverdueOrderForDebtNotificationAsync(
			IUnitOfWork uow,
			int maxClients,
			CancellationToken cancellationToken)
		{
			var currentDate = DateTime.UtcNow.Date;

			var deliveredOrderStatuses = new[]
			{
				OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed
			};

			Counterparty counterpartyAlias = null;
			CounterpartyContract contractAlias = null;
			Order orderAlias = null;
			Organization organizationAlias = null;
			BulkEmailEvent bulkEmailEventAlias = null;
			BulkEmailOrder bulkEmailOrderAlias = null;

			var lastEventIdSubQuery = QueryOver.Of<BulkEmailEvent>()
				.Where(bee2 => bee2.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Max<BulkEmailEvent>(bee2 => bee2.Id));

			var isClientUnsubscribedSubQuery = QueryOver.Of(() => bulkEmailEventAlias)
				.Where(() => bulkEmailEventAlias.Counterparty.Id == counterpartyAlias.Id)
				.Where(() => bulkEmailEventAlias.Type == BulkEmailEvent.BulkEmailEventType.Unsubscribing)
				.WithSubquery.WhereProperty(() => bulkEmailEventAlias.Id).Eq(lastEventIdSubQuery)
				.Select(bee => bee.Id);

			var alreadySentOrdersSubQuery = QueryOver.Of(() => bulkEmailOrderAlias)
				.Where(() => bulkEmailOrderAlias.Order.Id == orderAlias.Id)
				.Select(beo => beo.Id);

			var dateAddExpression = Projections.SqlFunction(
				new SQLFunctionTemplate(
					NHibernateUtil.DateTime,
					"DATE_ADD(?1, INTERVAL ?2 + 1 DAY)"
				),
				NHibernateUtil.DateTime,
				Projections.Property(() => orderAlias.DeliveryDate),
				Projections.Property(() => counterpartyAlias.DelayDaysForBuyers)
			);

			var query = uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinAlias(() => orderAlias.Contract, () => contractAlias)
				.JoinAlias(() => contractAlias.Organization, () => organizationAlias)
				.Where(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Where(() => orderAlias.DeliveryDate != null)
				.Where(() => orderAlias.OrderStatus.IsIn(deliveredOrderStatuses))
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless)
				.Where(() => counterpartyAlias.PersonType == PersonType.legal)
				.WhereNot(() => organizationAlias.DisableDebtMailing)
				.WhereNot(() => counterpartyAlias.DisableDebtMailing)
				.WhereNot(() => counterpartyAlias.IsArchive)
				 .WithSubquery.WhereExists(
					QueryOver.Of<Email>()
				 		.Where(email => email.Counterparty.Id == counterpartyAlias.Id)
				 		.Select(email => email.Id)
					)
				.Where(Restrictions.Le(dateAddExpression, currentDate))
				.WithSubquery.WhereNotExists(isClientUnsubscribedSubQuery)
				.WithSubquery.WhereNotExists(alreadySentOrdersSubQuery)
				.Take(maxClients); 

			var orders = await query.ListAsync(cancellationToken);

			var result = orders
				.Where(order => order.Client != null && order.Contract?.Organization != null)
				.Take(maxClients)
				.ToDictionary(
					order => order,
					order => (order.Client, order.Contract.Organization)
				);

			return result;
		}

		public IList<BulkEmailEventReason> GetUnsubscribingReasons(IUnitOfWork uow, IEmailSettings emailSettings, bool isForUnsubscribePage = false)
		{
			BulkEmailEventReason bulkEmailEventReasonAlias = null;

			var query = uow.Session.QueryOver(() => bulkEmailEventReasonAlias)
				.Where(x => !x.IsArchive);

			if(isForUnsubscribePage)
			{
				query.Where(x => !x.HideForUnsubscribePage);
			}

			query.OrderBy(x => x.Id == emailSettings.BulkEmailEventOtherReasonId);

			return query.List();
		}

		#region EmailType

		public IList<EmailType> GetEmailTypes(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<EmailType>().List<EmailType>();
		}

		public EmailType GetEmailTypeForReceipts(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<EmailType>()
				.Where(et => et.EmailPurpose == EmailPurpose.ForReceipts)
				.SingleOrDefault();
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

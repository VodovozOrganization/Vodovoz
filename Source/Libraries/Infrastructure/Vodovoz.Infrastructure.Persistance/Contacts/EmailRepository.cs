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
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Contacts;
using Vodovoz.Settings.Delivery;
using VodovozBusiness.Domain.Operations;
using VodovozBusiness.EntityRepositories.Nodes;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Contacts
{
	internal sealed class EmailRepository : IEmailRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEmailTypeSettings _emailTypeSettings;

		public EmailRepository(
			IUnitOfWorkFactory uowFactory,
			IEmailTypeSettings emailTypeSettings
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_emailTypeSettings = emailTypeSettings ?? throw new ArgumentNullException(nameof(emailTypeSettings));
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

		public CounterpartyBulkSubscribeNode GetCounterpartyBulkSubscribeInfoByGuidForUnsubscribing(IUnitOfWork uow, Guid emailGuid)
		{
			BulkEmailEvent bulkEmailEventAlias = null;
			CounterpartyEmail counterpartyEmailAlias = null;
			StoredEmail storedEmailAlias = null;
			CounterpartyBulkSubscribeNode resultAlias = null;

			var result = uow.Session.QueryOver(() => counterpartyEmailAlias)
				.JoinEntityAlias(() => bulkEmailEventAlias, 
					() => counterpartyEmailAlias.Counterparty.Id == bulkEmailEventAlias.Counterparty.Id
						&& counterpartyEmailAlias.Type == bulkEmailEventAlias.CounterpartyEmailType,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => counterpartyEmailAlias.StoredEmail, () => storedEmailAlias)
				.Where(() => storedEmailAlias.Guid == emailGuid)
				.OrderBy(Projections.Conditional(
					Restrictions.IsNotNull(Projections.Property(() => bulkEmailEventAlias.ActionTime)),
					Projections.Property(() => bulkEmailEventAlias.ActionTime),
					Projections.Constant(DateTime.MinValue)
				)).Desc
				.SelectList(list => list
					.Select(() => counterpartyEmailAlias.Counterparty.Id).WithAlias(() => resultAlias.CounterpartyId)					
					.Select(() => bulkEmailEventAlias.EventType).WithAlias(() => resultAlias.BulkEmailEventType)
					.Select(() => counterpartyEmailAlias.Type).WithAlias(() => resultAlias.CounterpartyEmailType))
				.TransformUsing(Transformers.AliasToBean<CounterpartyBulkSubscribeNode>())
				.Take(1)
				.List<CounterpartyBulkSubscribeNode>()
				.SingleOrDefault();

			return result?.BulkEmailEventType == BulkEmailEventType.Unsubscribing
				? null
				: result;
		}

		public BulkEmailEvent GetLastBulkEmailEvent(IUnitOfWork uow, int counterpartyId, CounterpartyEmailType? counterpartyEmailType = null)
		{
			BulkEmailEvent bulkEmailEventAlias = null;

			var query = uow.Session.QueryOver(() => bulkEmailEventAlias)
				.Where(() => bulkEmailEventAlias.Counterparty.Id == counterpartyId);

			if(counterpartyEmailType != null)
			{
				query.Where(() => bulkEmailEventAlias.CounterpartyEmailType == counterpartyEmailType);
			}

			return query
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

		public async Task<Dictionary<Order, (Counterparty Counterparty, Organization Organization)>> GetAllOverdueOrderForDebtNotificationAsync(
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
			OrderItem orderItemAlias = null;

			var lastEventIdSubQuery = QueryOver.Of<BulkEmailEvent>()
				.Where(bee2 => bee2.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Max<BulkEmailEvent>(bee2 => bee2.Id));

			var isClientUnsubscribedSubQuery = QueryOver.Of(() => bulkEmailEventAlias)
				.Where(() => bulkEmailEventAlias.Counterparty.Id == counterpartyAlias.Id)
				.Where(() => bulkEmailEventAlias.EventType == BulkEmailEventType.Unsubscribing)
				.WithSubquery.WhereProperty(() => bulkEmailEventAlias.Id).Eq(lastEventIdSubQuery)
				.Select(bee => bee.Id);

			var alreadySentOrdersSubQuery = QueryOver.Of(() => bulkEmailOrderAlias)
				.Where(() => bulkEmailOrderAlias.Order.Id == orderAlias.Id)
				.Select(beo => beo.Id);

			var orderItemsSumSubquery = QueryOver.Of(() => orderItemAlias)
			   .Where(() => orderItemAlias.Order.Id == orderAlias.Id)
			   .Select(Projections.SqlFunction(
				   new SQLFunctionTemplate(NHibernateUtil.Decimal, "COALESCE(SUM(?1 * IFNULL(?2, ?3) - ?4), 0)"),
				   NHibernateUtil.Decimal,
				   Projections.Property(() => orderItemAlias.Price),
				   Projections.Property(() => orderItemAlias.ActualCount),
				   Projections.Property(() => orderItemAlias.Count),
				   Projections.Property(() => orderItemAlias.DiscountMoney)
			   ));

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
				.Where(() => counterpartyAlias.CloseDeliveryDebtType == null)
				.WhereNot(() => organizationAlias.DisableDebtMailing)
				.WhereNot(() => counterpartyAlias.DisableDebtMailing)
				.WhereNot(() => counterpartyAlias.IsArchive)
				.WithSubquery.WhereExists(
					QueryOver.Of<Email>()
				 		.Where(email => email.Counterparty.Id == counterpartyAlias.Id)
						.And(email => email.EmailType == null || email.EmailType != null && email.EmailType.Id != _emailTypeSettings.ArchiveId)
				 		.Select(email => email.Id)
					)
				.Where(Restrictions.Le(dateAddExpression, currentDate))
				.WithSubquery.WhereNotExists(isClientUnsubscribedSubQuery)
				.WithSubquery.WhereNotExists(alreadySentOrdersSubQuery)
				.Where(Restrictions.Gt(
					Projections.SubQuery(orderItemsSumSubquery),
					0m
				))
				.Take(maxClients); 

			var orders = await query.ListAsync(cancellationToken);

			var result = orders
				.Where(order => order.Client != null && order.Contract?.Organization != null)
				.Take(maxClients)
				.ToDictionary(
					order => order,
					order => (Counterparty: order.Client, order.Contract.Organization)
				);

			return result;
		}

		public async Task<IList<OrderWithDebtNode>> GetAllOverdueOrdersForDebtNotificationAsync(
			IUnitOfWork uow,
			int maxClients,
			CancellationToken cancellationToken)
		{
			var currentDate = DateTime.UtcNow.Date;
			var today = DateTime.Today;
			var cashlessPaymentStart = new DateTime(2020, 8, 12);

			var deliveredOrderStatuses = new[]
			{
				OrderStatus.Shipped, 
				OrderStatus.UnloadingOnStock, 
				OrderStatus.Closed
			};

			Counterparty counterpartyAlias = null;
			CounterpartyContract contractAlias = null;
			Order orderAlias = null;
			Organization organizationAlias = null;
			BulkEmailEvent bulkEmailEventAlias = null;
			OrderItem orderItemAlias = null;
			PaymentItem paymentItemAlias = null;
			CashlessMovementOperation cashlessMovementOperationAlias = null;
			CounterpartyEmail counterpartyEmailAlias = null;
			StoredEmail storedEmailAlias = null;
			OrderWithDebtNode resultAlias = null;

			var emailSentTodaySubQuery = QueryOver.Of(() => counterpartyEmailAlias)
				.JoinAlias(() => counterpartyEmailAlias.StoredEmail, () => storedEmailAlias)
				.Where(() => counterpartyEmailAlias.Counterparty.Id == counterpartyAlias.Id)
				.Where(() => counterpartyEmailAlias.Type == CounterpartyEmailType.InformationLetter)
				.Where(() => storedEmailAlias.SendDate < today.AddDays(1))
				.Select(Projections.Property(() => counterpartyEmailAlias.Id));

			var lastEventIdSubQuery = QueryOver.Of<BulkEmailEvent>()
				.Where(bee2 => bee2.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Max<BulkEmailEvent>(bee2 => bee2.Id));

			var isClientUnsubscribedSubQuery = QueryOver.Of(() => bulkEmailEventAlias)
				.Where(() => bulkEmailEventAlias.Counterparty.Id == counterpartyAlias.Id)
				.Where(() => bulkEmailEventAlias.EventType == BulkEmailEventType.Unsubscribing)
				.WithSubquery.WhereProperty(() => bulkEmailEventAlias.Id).Eq(lastEventIdSubQuery)
				.Select(bee => bee.Id);

			var orderItemsSumSubquery = QueryOver.Of(() => orderItemAlias)
			   .Where(() => orderItemAlias.Order.Id == orderAlias.Id)
			   .Select(Projections.SqlFunction(
				   new SQLFunctionTemplate(NHibernateUtil.Decimal, "COALESCE(SUM(?1 * IFNULL(?2, ?3) - ?4), 0)"),
				   NHibernateUtil.Decimal,
				   Projections.Property(() => orderItemAlias.Price),
				   Projections.Property(() => orderItemAlias.ActualCount),
				   Projections.Property(() => orderItemAlias.Count),
				   Projections.Property(() => orderItemAlias.DiscountMoney)
			   ));

			var orderPaymentsSumSubquery = QueryOver.Of(() => paymentItemAlias)
			   .JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovementOperationAlias)
			   .Where(() => paymentItemAlias.Order.Id == orderAlias.Id)
			   .Where(() => cashlessMovementOperationAlias.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
			   .Select(Projections.Sum(() => cashlessMovementOperationAlias.Expense));

			var debtProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0) - IFNULL(?2, 0)"),
				NHibernateUtil.Decimal,
				Projections.SubQuery(orderItemsSumSubquery),
				Projections.SubQuery(orderPaymentsSumSubquery)
			);

			var dateAddExpression = Projections.SqlFunction(
				new SQLFunctionTemplate(
					NHibernateUtil.DateTime,
					"DATE_ADD(?1, INTERVAL ?2 + 1 DAY)"
				),
				NHibernateUtil.DateTime,
				Projections.Property(() => orderAlias.DeliveryDate),
				Projections.Property(() => counterpartyAlias.DelayDaysForBuyers)
			);

			// из-за того, что версия NHibernate не умеет работать с Take()
			// внутри подзапросов .WithSubquery, то пришлось повторять много кода
			var topClientsQuery = uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinAlias(() => orderAlias.Contract, () => contractAlias)
				.JoinAlias(() => contractAlias.Organization, () => organizationAlias)
				.Where(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Where(() => orderAlias.DeliveryDate != null)
				.Where(() => orderAlias.DeliveryDate > cashlessPaymentStart)
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless)
				.Where(() => orderAlias.OrderStatus.IsIn(deliveredOrderStatuses))
				.Where(() => counterpartyAlias.PersonType == PersonType.legal)
				.Where(() => counterpartyAlias.CloseDeliveryDebtType == null)
				.Where(() => organizationAlias.EmailForInformationLetters != null)
				.WhereNot(() => organizationAlias.DisableDebtMailing)
				.WhereNot(() => counterpartyAlias.DisableDebtMailing)
				.WhereNot(() => counterpartyAlias.IsArchive)
				.Where(Restrictions.Le(dateAddExpression, currentDate))
				.Where(Restrictions.Gt(Projections.SubQuery(orderItemsSumSubquery), 0m))
				.WithSubquery.WhereExists(
					QueryOver.Of<Email>()
						.Where(email => email.Counterparty.Id == counterpartyAlias.Id)
						.And(email => email.EmailType == null || email.EmailType.Id != _emailTypeSettings.ArchiveId)
						.Select(email => email.Id)
				)
				.WithSubquery.WhereNotExists(emailSentTodaySubQuery)
				.WithSubquery.WhereNotExists(isClientUnsubscribedSubQuery)
				.Select(Projections.Distinct(Projections.Property(() => counterpartyAlias.Id)))
				.OrderBy(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "RAND()"),
					NHibernateUtil.String))
				.Asc
				.Take(maxClients);

			var topClientIds = (await topClientsQuery.ListAsync<object>(cancellationToken))
				.Select(id => (int)id)
				.ToList();

			if(!topClientIds.Any())
			{
				return new List<OrderWithDebtNode>();
			}

			var result = await uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinAlias(() => orderAlias.Contract, () => contractAlias)
				.JoinAlias(() => contractAlias.Organization, () => organizationAlias)
				.Where(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Where(() => orderAlias.DeliveryDate != null)
				.Where(() => orderAlias.DeliveryDate > cashlessPaymentStart)
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless)
				.Where(() => orderAlias.OrderStatus.IsIn(deliveredOrderStatuses))
				.Where(() => organizationAlias.EmailForInformationLetters != null)
				.WhereNot(() => organizationAlias.DisableDebtMailing)
				.Where(Restrictions.Le(dateAddExpression, currentDate))
				.Where(Restrictions.Gt(Projections.SubQuery(orderItemsSumSubquery), 0m))
				.WhereRestrictionOn(() => counterpartyAlias.Id).IsIn(topClientIds)
				.Where(Restrictions.Gt(debtProjection, 0m))
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => organizationAlias.Id).WithAlias(() => resultAlias.OrganizationId)
					.Select(debtProjection).WithAlias(() => resultAlias.Debt)
				)
				.TransformUsing(Transformers.AliasToBean<OrderWithDebtNode>())
				.ListAsync<OrderWithDebtNode>(cancellationToken);

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

		/// <inheritdoc/>
		public int GetTodaySentLetterOfClaimsCount(IUnitOfWork uow)
		{
			var startOfDay = DateTime.Today;
			var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);

			var query =
				from letterOfClaimEmails in uow.Session.Query<LetterOfClaimEmail>()
				join storedEmail in uow.Session.Query<StoredEmail>()
					on letterOfClaimEmails.StoredEmail.Id equals storedEmail.Id
				where storedEmail.SendDate >= startOfDay && storedEmail.SendDate <= endOfDay
				select letterOfClaimEmails;

			return query.Count();
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

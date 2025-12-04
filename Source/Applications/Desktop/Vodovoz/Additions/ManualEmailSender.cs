using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NLog.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Services;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using VodovozBusiness.Domain.StoredEmails;
using VodovozInfrastructure.Configuration;

namespace Vodovoz.Additions
{
	public class ManualEmailSender
	{
		public ManualEmailSender()
		{
		}

		public void ResendEmailWithErrorSendingStatus(DateTime date)
		{
			using(var uowLocal = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				var configuration = uowLocal.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

				var dateCriterion = Projections.SqlFunction(
				   new SQLFunctionTemplate(
					   NHibernateUtil.Date,
					   "Date(?1)"
					  ),
				   NHibernateUtil.Date,
				   Projections.Property<StoredEmail>(x => x.SendDate)
				);
				ICriterion dateResctict = Restrictions.Eq(dateCriterion, date.Date);
				ICriterion dateResctictGe = Restrictions.Ge(dateCriterion, date.Date);

				#region OrderDocument

				BillDocumentEmail orderDocumentEmailAlias = null;
				BillDocumentEmail orderDocumentEmailInnerAlias = null;
				OrderDocument orderDocumentAlias = null;
				OrderDocument orderDocumentAliasInner = null;

				var resendedOrderDocumentQuery = QueryOver.Of<BillDocumentEmail>(() => orderDocumentEmailInnerAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(x => x.State != StoredEmailStates.SendingError)
					.And(dateResctictGe)
					.JoinAlias(() => orderDocumentEmailInnerAlias.OrderDocument, () => orderDocumentAliasInner)
					.Where(() => orderDocumentAliasInner.Order.Id == orderDocumentAlias.Order.Id)
					.Select(o => o.Id);

				var errorSendedOrderDocumentQuery = uowLocal.Session.QueryOver<BillDocumentEmail>(() => orderDocumentEmailAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(se => se.State == StoredEmailStates.SendingError)
					.And(dateResctict)
					.JoinAlias(() => orderDocumentEmailAlias.OrderDocument, () => orderDocumentAlias)
					.WithSubquery.WhereNotExists(resendedOrderDocumentQuery)
					.Future();

				#endregion

				#region Order OrderWithoutShipmentForDebt

				OrderWithoutShipmentForDebtEmail orderWithoutShipmentForDebtEmailAlias = null;
				OrderWithoutShipmentForDebtEmail orderWithoutShipmentForDebtEmailInnerAlias = null;
				OrderWithoutShipmentForDebt orderWithoutShipmentForDebtAlias = null;
				OrderWithoutShipmentForDebt orderWithoutShipmentForDebtAliasInner = null;

				var resendedOrderWithoutShipmentForDebtQuery = QueryOver.Of<OrderWithoutShipmentForDebtEmail>(() => orderWithoutShipmentForDebtEmailInnerAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(x => x.State != StoredEmailStates.SendingError)
					.And(dateResctictGe)
					.JoinAlias(() => orderWithoutShipmentForDebtEmailInnerAlias.OrderWithoutShipmentForDebt, () => orderWithoutShipmentForDebtAliasInner)
					.Where(() => orderWithoutShipmentForDebtAliasInner.Id == orderWithoutShipmentForDebtAlias.Id)
					.Select(o => o.Id);

				var errorSendedOrderWithoutShipmentForDebtEmailQuery = uowLocal.Session
					.QueryOver<OrderWithoutShipmentForDebtEmail>(() => orderWithoutShipmentForDebtEmailAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(se => se.State == StoredEmailStates.SendingError)
					.And(dateResctict)
					.JoinAlias(() => orderWithoutShipmentForDebtEmailAlias.OrderWithoutShipmentForDebt, () => orderWithoutShipmentForDebtAlias)
					.WithSubquery.WhereNotExists(resendedOrderWithoutShipmentForDebtQuery)
					.Future();

				#endregion

				#region Order OrderWithoutShipmentForAdvancePayment

				OrderWithoutShipmentForAdvancePaymentEmail orderWithoutShipmentForAdvancePaymentEmailAlias = null;
				OrderWithoutShipmentForAdvancePaymentEmail orderWithoutShipmentForAdvancePaymentEmailInnerAlias = null;
				OrderWithoutShipmentForAdvancePayment orderWithoutShipmentForAdvancePaymentAlias = null;
				OrderWithoutShipmentForAdvancePayment orderWithoutShipmentForAdvancePaymentAliasInner = null;

				var resendedOrderWithoutShipmentForAdvancePaymentQuery = QueryOver.Of<OrderWithoutShipmentForAdvancePaymentEmail>(() => orderWithoutShipmentForAdvancePaymentEmailInnerAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(x => x.State != StoredEmailStates.SendingError)
					.And(dateResctictGe)
					.JoinAlias(() => orderWithoutShipmentForAdvancePaymentEmailInnerAlias.OrderWithoutShipmentForAdvancePayment, () => orderWithoutShipmentForAdvancePaymentAliasInner)
					.Where(() => orderWithoutShipmentForAdvancePaymentAliasInner.Id == orderWithoutShipmentForAdvancePaymentAlias.Id)
					.Select(o => o.Id);

				var errorSendedOrderWithoutShipmentForAdvancePaymentEmailQuery = uowLocal.Session
					.QueryOver<OrderWithoutShipmentForAdvancePaymentEmail>(() => orderWithoutShipmentForAdvancePaymentEmailAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(se => se.State == StoredEmailStates.SendingError)
					.And(dateResctict)
					.JoinAlias(() => orderWithoutShipmentForAdvancePaymentEmailAlias.OrderWithoutShipmentForAdvancePayment, () => orderWithoutShipmentForAdvancePaymentAlias)
					.WithSubquery.WhereNotExists(resendedOrderWithoutShipmentForAdvancePaymentQuery)
					.Future();

				#endregion

				#region Order OrderWithoutShipmentForPayment

				OrderWithoutShipmentForPaymentEmail orderWithoutShipmentForPaymentEmailAlias = null;
				OrderWithoutShipmentForPaymentEmail orderWithoutShipmentForPaymentEmailInnerAlias = null;
				OrderWithoutShipmentForPayment orderWithoutShipmentForPaymentAlias = null;
				OrderWithoutShipmentForPayment orderWithoutShipmentForPaymentAliasInner = null;

				var resendedOrderWithoutShipmentForPaymentQuery = QueryOver.Of<OrderWithoutShipmentForPaymentEmail>(() => orderWithoutShipmentForPaymentEmailInnerAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(x => x.State != StoredEmailStates.SendingError)
					.And(dateResctictGe)
					.JoinAlias(() => orderWithoutShipmentForPaymentEmailInnerAlias.OrderWithoutShipmentForPayment, () => orderWithoutShipmentForPaymentAliasInner)
					.Where(() => orderWithoutShipmentForPaymentAliasInner.Id == orderWithoutShipmentForPaymentAlias.Id)
					.Select(o => o.Id);

				var errorSendedOrderWithoutShipmentForPaymentEmailQuery = uowLocal.Session
					.QueryOver<OrderWithoutShipmentForPaymentEmail>(() => orderWithoutShipmentForPaymentEmailAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(se => se.State == StoredEmailStates.SendingError)
					.And(dateResctict)
					.JoinAlias(() => orderWithoutShipmentForPaymentEmailAlias.OrderWithoutShipmentForPayment, () => orderWithoutShipmentForPaymentAlias)
					.WithSubquery.WhereNotExists(resendedOrderWithoutShipmentForPaymentQuery)
					.Future();

				#endregion

				var errorSendedCounterpartyEmails = errorSendedOrderDocumentQuery
					.Union<CounterpartyEmail>(errorSendedOrderWithoutShipmentForDebtEmailQuery)
					.Union<CounterpartyEmail>(errorSendedOrderWithoutShipmentForAdvancePaymentEmailQuery)
					.Union<CounterpartyEmail>(errorSendedOrderWithoutShipmentForPaymentEmailQuery);

				var errorSendedCounterpartyEmailsList = errorSendedCounterpartyEmails.ToList();

				foreach(var sendedEmail in errorSendedCounterpartyEmailsList)
				{
					try
					{

						using(var unitOfWork = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("StoredEmail"))
						{
							var storedEmail = new StoredEmail
							{
								State = StoredEmailStates.PreparingToSend,
								Author = sendedEmail.StoredEmail.Author,
								ManualSending = true,
								SendDate = DateTime.Now,
								StateChangeDate = DateTime.Now,
								Subject = sendedEmail.StoredEmail.Subject,
								RecipientAddress = sendedEmail.StoredEmail.RecipientAddress
							};

							unitOfWork.Save(storedEmail);

							switch(sendedEmail.Type)
							{
								case CounterpartyEmailType.BillDocument:
									var orderDocumentEmail = new BillDocumentEmail
									{
										StoredEmail = storedEmail,
										Counterparty = sendedEmail.Counterparty,
										OrderDocument = ((BillDocumentEmail)sendedEmail).OrderDocument
									};

									unitOfWork.Save(orderDocumentEmail);

									break;

								case CounterpartyEmailType.EquipmentTransfer:
									var equipmentTransferDocumentEmail = new EquipmentTransferDocumentEmail
									{
										StoredEmail = storedEmail,
										Counterparty = sendedEmail.Counterparty,
										OrderDocument = ((EquipmentTransferDocumentEmail)sendedEmail).OrderDocument
									};

									unitOfWork.Save(equipmentTransferDocumentEmail);

									break;

								case CounterpartyEmailType.UpdDocument:
									var updDocumentEmail = new UpdDocumentEmail()
									{
										StoredEmail = storedEmail,
										Counterparty = sendedEmail.Counterparty,
										OrderDocument = ((UpdDocumentEmail)sendedEmail).OrderDocument
									};

									unitOfWork.Save(updDocumentEmail);

									break;

								case CounterpartyEmailType.OrderWithoutShipmentForDebt:
									var orderWithoutShipmentForDebtEmail = new OrderWithoutShipmentForDebtEmail()
									{
										StoredEmail = storedEmail,
										Counterparty = sendedEmail.Counterparty,
										OrderWithoutShipmentForDebt = (OrderWithoutShipmentForDebt) sendedEmail.EmailableDocument
									};

									unitOfWork.Save(orderWithoutShipmentForDebtEmail);

									break;

								case CounterpartyEmailType.OrderWithoutShipmentForAdvancePayment:
									var orderWithoutShipmentForAdvancePaymentEmail = new OrderWithoutShipmentForAdvancePaymentEmail()
									{
										StoredEmail = storedEmail,
										Counterparty = sendedEmail.Counterparty,
										OrderWithoutShipmentForAdvancePayment = (OrderWithoutShipmentForAdvancePayment)sendedEmail.EmailableDocument
									};

									unitOfWork.Save(orderWithoutShipmentForAdvancePaymentEmail);

									break;

								case CounterpartyEmailType.OrderWithoutShipmentForPayment:
									var orderWithoutShipmentForPaymentEmail = new OrderWithoutShipmentForPaymentEmail()
									{
										StoredEmail = storedEmail,
										Counterparty = sendedEmail.Counterparty,
										OrderWithoutShipmentForPayment = (OrderWithoutShipmentForPayment)sendedEmail.EmailableDocument
									};

									unitOfWork.Save(orderWithoutShipmentForPaymentEmail);

									break;
							}

							unitOfWork.Commit();
						}
					}
					catch(Exception e)
					{
						Console.WriteLine($"Ошибка отправки { sendedEmail.Id } : { e.Message }");
					}
				}
			}
		}
	}
}

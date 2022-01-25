using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NLog.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
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
			IList<OrderDocumentEmail> errorSendedEmails;
			using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var configuration = uowLocal.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

				OrderDocumentEmail unsendedEmailAlias = null;

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

				var resendedQuery = QueryOver.Of<OrderDocumentEmail>()
					.Where(Restrictions.EqProperty(Projections.Property<OrderDocumentEmail>(ode => ode.Order.Id), Projections.Property(() => unsendedEmailAlias.Order.Id)))
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(x => x.State != StoredEmailStates.SendingError)
					.Where(dateResctictGe)
					.Select(Projections.Count(Projections.Id()));

				errorSendedEmails = uowLocal.Session.QueryOver<OrderDocumentEmail>(() => unsendedEmailAlias)
					.JoinQueryOver(ode => ode.StoredEmail)
					.Where(se => se.State == StoredEmailStates.SendingError)
					.Where(dateResctict)
					.WithSubquery.WhereValue(0).Eq(resendedQuery)
					.List();

				foreach(var sendedEmail in errorSendedEmails)
				{
					if(!(sendedEmail.Order.OrderDocuments.FirstOrDefault(y => y.Type == OrderDocumentType.Bill) is BillDocument billDocument))
					{
						continue;
					}

					try
					{

						using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("StoredEmail"))
						{
							var storedEmail = new StoredEmail
							{
								State = StoredEmailStates.PreparingToSend,
								Author = sendedEmail.StoredEmail.Author,
								ManualSending = true,
								SendDate = DateTime.Now,
								StateChangeDate = DateTime.Now,
								RecipientAddress = sendedEmail.StoredEmail.RecipientAddress
							};

							unitOfWork.Save(storedEmail);

							var orderDocumentEmail = new OrderDocumentEmail
							{
								StoredEmail = storedEmail,
								Order = sendedEmail.Order,
								OrderDocument = sendedEmail.OrderDocument
							};

							unitOfWork.Save(orderDocumentEmail);

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

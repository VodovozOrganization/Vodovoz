using System;
using System.IO;
using EmailService;
using fyiReporting.RDL;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.UoW;
using QS.Report;
using Vodovoz.Parameters;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using System.Collections.Generic;
using System.Linq;
using fyiReporting.RdlGtkViewer;
using RdlEngine;

namespace Vodovoz.Additions
{
	public class ManualEmailSender
	{
		public ManualEmailSender()
		{
		}

		public void ResendEmailWithErrorSendingStatus(DateTime date)
		{
			IEmailService service = EmailServiceSetting.GetEmailService();
			if(service == null) {
				return;
			}

			IList<StoredEmail> errorSendedEmails;
			using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot()) {

				StoredEmail unsendedEmailAlias = null;
				StoredEmail alreadyResendedEmailAlias = null;

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

				var resendedQuery = QueryOver.Of<StoredEmail>()
					.Where(Restrictions.EqProperty(Projections.Property<StoredEmail>(x => x.Order.Id), Projections.Property(() => unsendedEmailAlias.Order.Id)))
					.Where(x => x.State != StoredEmailStates.SendingError)
					.Where(dateResctictGe)
					.Select(Projections.Count(Projections.Id()));
				
				errorSendedEmails = uowLocal.Session.QueryOver<StoredEmail>(() => unsendedEmailAlias)
					.Where(x => x.State == StoredEmailStates.SendingError)
					.Where(dateResctict)
					.WithSubquery.WhereValue(0).Eq(resendedQuery)
					.List();

				foreach(var sendedEmail in errorSendedEmails) {
					var billDocument = sendedEmail.Order.OrderDocuments.FirstOrDefault(y => y.Type == OrderDocumentType.Bill) as BillDocument;
					if(billDocument == null) {
						continue;
					}

					billDocument.HideSignature = false;
					ReportInfo ri = billDocument.GetReportInfo();

				   var billTemplate = billDocument.GetEmailTemplate();
					OrderEmail email = new OrderEmail {
						Title = string.Format("{0} {1}", billTemplate.Title, billDocument.Title),
						Text = billTemplate.Text,
						HtmlText = billTemplate.TextHtml,
						Recipient = new EmailContact("", sendedEmail.RecipientAddress),
						Sender = new EmailContact("vodovoz-spb.ru", new ParametersProvider().GetParameterValue("email_for_email_delivery")),
						Order = billDocument.Order.Id,
						OrderDocumentType = OrderDocumentType.Bill
					};
					foreach(var item in billTemplate.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}
					using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
						string billDate = billDocument.DocumentDate.HasValue ? "_" + billDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
						email.AddAttachment($"Bill_{billDocument.Order.Id}{billDate}.pdf", stream);
					}
					email.AuthorId = sendedEmail.Author.Id;
					email.ManualSending = sendedEmail.ManualSending ?? false;
				
					service.SendOrderEmail(email);
				}
			}
		}
	}
}

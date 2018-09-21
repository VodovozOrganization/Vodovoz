using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmailService;
using fyiReporting.RDL;
using fyiReporting.RdlGtkViewer;
using Gamma.GtkWidgets;
using QSEmailSending;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using QSSupportLib;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Repositories;
using Vodovoz.Repository;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SendDocumentByEmailView : WidgetOnDialogBase
	{
		private OrderDocument document;
		private bool canSend => document is IPrintableRDLDocument;

		public SendDocumentByEmailView()
		{
			this.Build();

			yvalidatedentryEmail.ValidationMode = QSWidgetLib.ValidationType.email;

			ytreeviewStoredEmails.ColumnsConfig = ColumnsConfigFactory
				.Create<StoredEmail>()
				.AddColumn("Дата").AddTextRenderer(x => x.SendDate.ToString("dd.MM.yyyy HH:mm"))
				.AddColumn("Почта").AddTextRenderer(x => x.RecipientAddress)
				.AddColumn("Статус").AddEnumRenderer(x => x.State)
				.RowCells()
				.Finish();

			this.Sensitive = EmailServiceSetting.CanSendEmail;
		}

		public void Update(OrderDocument document, string email)
		{
			yvalidatedentryEmail.Text = email;

			this.document = document;
			UpdateEmails();
		}

		private void UpdateEmails()
		{
			DateTime lastSendDate;
			List<StoredEmail> storedEmails;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				storedEmails = uow.Session.QueryOver<StoredEmail>()
				                  .Where(x => x.Order.Id == document.Order.Id)
				                  .Where(x => x.DocumentType == document.Type)
				                  .List().ToList();
			}
			ytreeviewStoredEmails.ItemsDataSource = storedEmails;
			buttonSendEmail.Sensitive = document.Type == OrderDocumentType.Bill && EmailRepository.CanSendByTimeout(yvalidatedentryEmail.Text, document.Order.Id);
		}

		protected void OnYtreeviewStoredEmailsCursorChanged(object sender, EventArgs e)
		{
			var selectedStoredEmail = ytreeviewStoredEmails.GetSelectedObject() as StoredEmail;
			if(selectedStoredEmail == null) {
				return;
			}
			labelDescription.Text = selectedStoredEmail.Description;
		}

		protected void OnButtonSendEmailClicked(object sender, EventArgs e)
		{
			SendDocument();
		}

		private void SendDocument()
		{
			var client = document.Order.Client;
			var rdlDoc = (document as IPrintableRDLDocument);

			if(rdlDoc == null) {
				MessageDialogWorks.RunErrorDialog("Невозможно распечатать данный тип документа");
				return;
			}
			if(client == null) {
				MessageDialogWorks.RunErrorDialog("Должен быть выбран клиент в заказе");
				return;
			}

			var organization = OrganizationRepository.GetCashlessOrganization(UnitOfWorkFactory.CreateWithoutRoot());
			if(organization == null) {
				MessageDialogWorks.RunErrorDialog("В параметрах базы не определена организация для безналичного расчета");
				return;
			}

			if(!MainSupport.BaseParameters.All.ContainsKey("email_for_email_delivery")) {
				MessageDialogWorks.RunErrorDialog("В параметрах базы не определена почта для рассылки");
				return;
			}

			if(string.IsNullOrWhiteSpace(yvalidatedentryEmail.Text)) {
				MessageDialogWorks.RunErrorDialog("Необходимо ввести адрес электронной почты");
				return;
			}

			Email email = CreateDocumentEmail(client.Name, organization.Name, document);
			if(email == null) {
				MessageDialogWorks.RunErrorDialog("Для данного типа документа не реализовано формирование письма");
				return;
			}

			IEmailService service = EmailServiceSetting.GetEmailService();
			if(service == null) {
				return;
			}
			var result = service.SendEmail(email);

			//Если произошла ошибка и письмо не отправлено
			string resultMessage = "";
			if(!result.Item1) {
				resultMessage = "Письмо не было отправлено! Причина:\n";
			}
			MessageDialogWorks.RunInfoDialog(resultMessage + result.Item2);

			UpdateEmails();
		}

		private Email CreateDocumentEmail(string clientName, string organizationName, OrderDocument document)
		{
			if(document.Type == OrderDocumentType.Bill) {
				var billDocument = document as BillDocument;
				var wasHideSignature = billDocument.HideSignature;
				billDocument.HideSignature = false;
				ReportInfo ri = billDocument.GetReportInfo();
				billDocument.HideSignature = wasHideSignature;
				            
				EmailTemplate template = billDocument.GetEmailTemplate();
				Email email = new Email();
				email.Title = string.Format("{0} {1}", template.Title, billDocument.Title);
				email.Text = template.Text;
				email.HtmlText = template.TextHtml;
				foreach(var item in template.Attachments) {
					email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
				}

				email.Recipient = new EmailContact(clientName, yvalidatedentryEmail.Text);
				email.Sender = new EmailContact(organizationName, MainSupport.BaseParameters.All["email_for_email_delivery"]);
				email.Order = document.Order.Id;
				email.OrderDocumentType = document.Type;
				using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
					email.AddAttachment(billDocument.Name + ".pdf", stream);				
				}
				return email;
			}else {
				//для других документов не реализована отправка почты
				return null;
			}
		}

		protected void OnButtonRefreshEmailListClicked(object sender, EventArgs e)
		{
			UpdateEmails();
		}
	}
}

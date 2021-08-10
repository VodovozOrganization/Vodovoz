using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmailService;
using fyiReporting.RDL;
using Gamma.GtkWidgets;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using Vodovoz.Parameters;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using NHibernate.Criterion;
using RdlEngine;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SendDocumentByEmailView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private OrderDocument document;
		private bool canSend => document is IPrintableRDLDocument;
		private IEmailRepository emailRepository;

		public SendDocumentByEmailView()
		{
			this.Build();

			emailRepository = new EmailRepository();

			yvalidatedentryEmail.ValidationMode = QSWidgetLib.ValidationType.email;

			ytreeviewStoredEmails.ColumnsConfig = ColumnsConfigFactory
				.Create<StoredEmail>()
				.AddColumn("Дата").AddTextRenderer(x => x.SendDate.ToString("dd.MM.yyyy HH:mm"))
				.AddColumn("Почта").AddTextRenderer(x => x.RecipientAddress)
				.AddColumn("Статус").AddEnumRenderer(x => x.State)
				.RowCells()
				.Finish();

			this.Sensitive = EmailServiceSetting.SendingAllowed;
		}

		public void Update(OrderDocument document, string email)
		{
			yvalidatedentryEmail.Text = email;

			this.document = document;
			UpdateEmails();
		}

		private void UpdateEmails()
		{
			List<StoredEmail> storedEmails;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				storedEmails = uow.Session.QueryOver<StoredEmail>()
					.Where(Restrictions.Eq(
						Projections.Property<StoredEmail>(x => x.Order.Id),
						document.Order.Id))
					.And(Restrictions.Eq(
						Projections.Property<StoredEmail>(x => x.DocumentType),
						document.Type))
					.List().ToList();
			}
			ytreeviewStoredEmails.ItemsDataSource = storedEmails;
			buttonSendEmail.Sensitive = document.Type == OrderDocumentType.Bill && emailRepository.CanSendByTimeout(yvalidatedentryEmail.Text, document.Order.Id, OrderDocumentType.Bill) && document.Order.Id > 0;
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
				MessageDialogHelper.RunErrorDialog("Невозможно распечатать данный тип документа");
				return;
			}

			if(document.Order.Id == 0){
				if(!MessageDialogHelper.RunQuestionDialog("Для отправки необходимо сохранить заказ, сохранить сейчас?")) {
					return;
				}
				if(!(MyOrmDialog as OrderDlg).Save()) {
					return;
				}
			}

			if(client == null) {
				MessageDialogHelper.RunErrorDialog("Должен быть выбран клиент в заказе");
				return;
			}

			if(!new ParametersProvider().ContainsParameter("email_for_email_delivery")) {
				MessageDialogHelper.RunErrorDialog("В параметрах базы не определена почта для рассылки");
				return;
			}

			if(string.IsNullOrWhiteSpace(yvalidatedentryEmail.Text)) {
				MessageDialogHelper.RunErrorDialog("Необходимо ввести адрес электронной почты");
				return;
			}

			OrderEmail email = CreateDocumentEmail("", "vodovoz-spb.ru", document);
			if(email == null) {
				MessageDialogHelper.RunErrorDialog("Для данного типа документа не реализовано формирование письма");
				return;
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var employee = new EmployeeRepository().GetEmployeeForCurrentUser(uow);
				email.AuthorId = employee != null ? employee.Id : 0;
				email.ManualSending = true;
			}

			IEmailService service = EmailServiceSetting.GetEmailService();
			if(service == null) {
				return;
			}
			var result = service.SendOrderEmail(email);

			//Если произошла ошибка и письмо не отправлено
			string resultMessage = "";
			if(!result.Item1) {
				resultMessage = "Письмо не было отправлено! Причина:\n";
			}
			MessageDialogHelper.RunInfoDialog(resultMessage + result.Item2);

			UpdateEmails();
		}

		private OrderEmail CreateDocumentEmail(string clientName, string organizationName, OrderDocument document)
		{
			if(document.Type == OrderDocumentType.Bill) {
				var billDocument = document as BillDocument;
				var wasHideSignature = billDocument.HideSignature;
				billDocument.HideSignature = false;
				ReportInfo ri = billDocument.GetReportInfo();
				billDocument.HideSignature = wasHideSignature;
				            
				EmailTemplate template = billDocument.GetEmailTemplate();
				OrderEmail email = new OrderEmail();
				email.Title = string.Format("{0} {1}", template.Title, billDocument.Title);
				email.Text = template.Text;
				email.HtmlText = template.TextHtml;
				foreach(var item in template.Attachments) {
					email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
				}

				email.Recipient = new EmailContact(clientName, yvalidatedentryEmail.Text);
				email.Sender = new EmailContact(organizationName, new ParametersProvider().GetParameterValue("email_for_email_delivery"));
				email.Order = document.Order.Id;
				email.OrderDocumentType = document.Type;
				using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
					string billDate = billDocument.DocumentDate.HasValue ? "_" + billDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
					email.AddAttachment($"Bill_{billDocument.Order.Id}{billDate}.pdf", stream);				
				}
				return email;
			}else {
				//для других документов не реализована отправка почты
				return null;
			}
		}

		protected void OnButtonRefreshEmailListClicked(object sender, EventArgs e)
		{
			if(document?.Order == null)
				return;

			UpdateEmails();
		}
	}
}

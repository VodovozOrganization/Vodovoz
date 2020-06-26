using System.Collections.Generic;
using System.Linq;
using EmailService;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QS.ViewModels;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using System.Data.Bindings.Collections.Generic;
using QS.Report;
using QS.Dialog.GtkUI;
using Vodovoz.Parameters;
using Vodovoz.EntityRepositories.Employees;
using System.IO;
using fyiReporting.RdlGtkViewer;
using fyiReporting.RDL;
using QS.Commands;
using System;
using System.Collections.ObjectModel;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Dialogs.Email
{
	public class SendDocumentByEmailViewModel : UoWWidgetViewModelBase
	{
		private string emailString;
		public string EmailString {
			get => emailString;
			set => SetField(ref emailString, value);
		}

		private string description;
		public string Description {
			get => description;
			set => SetField(ref description, value);
		}

		private bool btnSendEmailSensitive;
		public bool BtnSendEmailSensitive {
			get => btnSendEmailSensitive;
			set => SetField(ref btnSendEmailSensitive, value);
		}

		private object selectedObj;
		public object SelectedObj {
			get => selectedObj;
			set => SetField(ref selectedObj, value);
		}

		private readonly IEmailRepository emailRepository;
		private readonly IEmployeeRepository employeeRepository;
		private IDocument Document { get; set; }

		public List<StoredEmail> StoredEmails { get; set; }
		
		public ObservableCollection<StoredEmail> ListEmails { get; set; }

		public DelegateCommand SendEmailCommand { get; private set; }

		public DelegateCommand RefreshEmailListCommand { get; private set; }
		
		public SendDocumentByEmailViewModel(IEmailRepository emailRepository, IEmployeeRepository employeeRepository, IUnitOfWork uow = null)
		{
			this.emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			StoredEmails = new List<StoredEmail>();
			UoW = uow;

			CreateCommands();
		}

		private void CreateCommands()
		{
			CreateSendEmailCommand();
			CreateRefreshEmailListCommand();
		}

		private void CreateSendEmailCommand()
		{
			SendEmailCommand = new DelegateCommand(
				SaveEntity,
				() => !string.IsNullOrEmpty(EmailString)
			);
		}

		private void SaveEntity()
		{
			switch(Document.Type)
			{
				case OrderDocumentType.BillWithoutShipmentForPayment:
					UoW.Save(Document as OrderWithoutShipmentForPayment);
					break;
			}
		}

		private void CreateRefreshEmailListCommand()
		{
			RefreshEmailListCommand = new DelegateCommand(
				UpdateEmails,
				() => true//Document?.Order != null
			);
		}

		public void Update(IDocument document, string email)
		{
			EmailString = email;

			Document = document;
			UpdateEmails();
		}

		private void UpdateEmails()
		{
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				switch (Document.Type)
				{
					case OrderDocumentType.Bill :
						StoredEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.Order.Id == Document.Order.Id)
							.And(x => x.DocumentType == Document.Type)
							.List().ToList();
						
						BtnSendEmailSensitive = Document.Type == OrderDocumentType.Bill
						                        && emailRepository.CanSendByTimeout(EmailString, Document.Order.Id, Document.Type) 
						                        && Document.Order.Id > 0;
						break;
					case OrderDocumentType.BillWithoutShipmentForDebt:
						StoredEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForDebt.Id == Document.Id)
							.And(x => x.DocumentType == Document.Type)
							.List().ToList();
						
						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWithoutShipmentForDebt
						                        && emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					case OrderDocumentType.BillWithoutShipmentForAdvancePayment:
						StoredEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForAdvancePayment.Id == Document.Id)
							.And(x => x.DocumentType == Document.Type)
							.List().ToList();
						
						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWithoutShipmentForAdvancePayment
						                        && emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					case OrderDocumentType.BillWithoutShipmentForPayment:
						StoredEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForPayment.Id == Document.Id)
							.And(x => x.DocumentType == Document.Type)
							.List().ToList();
						
						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWithoutShipmentForPayment
						                        && emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
				}
			}
			
			/*BtnSendEmailSensitive = (Document.Type == OrderDocumentType.Bill 
									|| Document.Type == OrderDocumentType.BillWithoutShipmentForDebt 
									|| Document.Type == OrderDocumentType.BillWithoutShipmentForAdvancePayment 
									|| Document.Type == OrderDocumentType.BillWithoutShipmentForPayment) 
									&& emailRepository.CanSendByTimeout(EmailString, Document.Order.Id, Document.Type) 
									&& Document.Order.Id > 0;*/
		}

		private void SendDocument()
		{
			var client = Document.Order?.Client;
			var rdlDoc = Document as IPrintableRDLDocument;

			if(rdlDoc == null) {
				MessageDialogHelper.RunErrorDialog("Невозможно распечатать данный тип документа");
				return;
			}

			if(Document.Type == OrderDocumentType.Bill && Document.Order?.Id == 0) {
				MessageDialogHelper.RunErrorDialog("Для отправки необходимо сохранить заказ."); 
				return;
				
				/*if(!(DialogHelper.FindParentUowDialog(this) as OrderDlg).Save()) {
					return;
				}*/
			}

			if(Document.Type == OrderDocumentType.Bill && client == null) {
				MessageDialogHelper.RunErrorDialog("Должен быть выбран клиент в заказе");
				return;
			}

			if(!ParametersProvider.Instance.ContainsParameter("email_for_email_delivery")) {
				MessageDialogHelper.RunErrorDialog("В параметрах базы не определена почта для рассылки");
				return;
			}

			if(string.IsNullOrWhiteSpace(EmailString)) {
				MessageDialogHelper.RunErrorDialog("Необходимо ввести адрес электронной почты");
				return;
			}

			EmailService.Email email = CreateDocumentEmail("", "vodovoz-spb.ru", Document);
			if(email == null) {
				MessageDialogHelper.RunErrorDialog("Для данного типа документа не реализовано формирование письма");
				return;
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var employee = employeeRepository.GetEmployeeForCurrentUser(uow);
				email.AuthorId = employee != null ? employee.Id : 0;
				email.ManualSending = true;
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
			MessageDialogHelper.RunInfoDialog(resultMessage + result.Item2);

			UpdateEmails();
		}

		private EmailService.Email CreateDocumentEmail(string clientName, string organizationName, IDocument document)
		{
			bool wasHideSignature;
			ReportInfo ri = null;
			EmailTemplate template = null;
			EmailService.Email email = null;

			switch(document.Type) {

				case OrderDocumentType.Bill	:
					var billDocument = document as BillDocument;
					wasHideSignature = billDocument.HideSignature;
					billDocument.HideSignature = false;
					ri = billDocument.GetReportInfo();
					billDocument.HideSignature = wasHideSignature;

					template = billDocument.GetEmailTemplate();
					email = new EmailService.Email();
					email.Title = string.Format($"{template.Title} {billDocument.Title}");
					email.Text = template.Text;
					email.HtmlText = template.TextHtml;

					foreach(var item in template.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}

					email.Recipient = new EmailContact(clientName, EmailString);
					email.Sender = new EmailContact(organizationName, ParametersProvider.Instance.GetParameterValue("email_for_email_delivery"));
					email.Order = document.Order.Id;
					email.OrderDocumentType = document.Type;

					using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
						string billDate = billDocument.DocumentDate.HasValue ? "_" + billDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
						email.AddAttachment($"Bill_{billDocument.Order.Id}{billDate}.pdf", stream);
					}
					return email;
				case OrderDocumentType.BillWithoutShipmentForDebt:
					var billWSFDDocument = document as OrderWithoutShipmentForDebt;
					wasHideSignature = billWSFDDocument.HideSignature;
					billWSFDDocument.HideSignature = false;
					ri = billWSFDDocument.GetReportInfo();
					billWSFDDocument.HideSignature = wasHideSignature;

					template = billWSFDDocument.GetEmailTemplate();
					email = new EmailService.Email();
					email.Title = string.Format($"{template.Title} {billWSFDDocument.Title}");
					email.Text = template.Text;
					email.HtmlText = template.TextHtml;

					foreach(var item in template.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}

					email.Recipient = new EmailContact(clientName, EmailString);
					email.Sender = new EmailContact(organizationName, ParametersProvider.Instance.GetParameterValue("email_for_email_delivery"));
					email.Order = document.Id;
					email.OrderDocumentType = document.Type;

					using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
						string billDate = billWSFDDocument.DocumentDate.HasValue ? "_" + billWSFDDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
						email.AddAttachment($"Bill_{billWSFDDocument.Id}{billDate}.pdf", stream);
					}
					return email;
				case OrderDocumentType.BillWithoutShipmentForAdvancePayment:
					var billWSFAPDocument = document as OrderWithoutShipmentForAdvancePayment;
					wasHideSignature = billWSFAPDocument.HideSignature;
					billWSFAPDocument.HideSignature = false;
					ri = billWSFAPDocument.GetReportInfo();
					billWSFAPDocument.HideSignature = wasHideSignature;

					template = billWSFAPDocument.GetEmailTemplate();
					email = new EmailService.Email();
					email.Title = string.Format($"{template.Title} {billWSFAPDocument.Title}");
					email.Text = template.Text;
					email.HtmlText = template.TextHtml;

					foreach(var item in template.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}

					email.Recipient = new EmailContact(clientName, EmailString);
					email.Sender = new EmailContact(organizationName, ParametersProvider.Instance.GetParameterValue("email_for_email_delivery"));
					email.Order = document.Id;
					email.OrderDocumentType = document.Type;

					using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
						string billDate = billWSFAPDocument.DocumentDate.HasValue ? "_" + billWSFAPDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
						email.AddAttachment($"Bill_{billWSFAPDocument.Id}{billDate}.pdf", stream);
					}
					return email;
				case OrderDocumentType.BillWithoutShipmentForPayment:
					var billWSFPDocument = document as OrderWithoutShipmentForPayment;
					wasHideSignature = billWSFPDocument.HideSignature;
					billWSFPDocument.HideSignature = false;
					ri = billWSFPDocument.GetReportInfo();
					billWSFPDocument.HideSignature = wasHideSignature;

					template = billWSFPDocument.GetEmailTemplate();
					email = new EmailService.Email();
					email.Title = string.Format($"{template.Title} {billWSFPDocument.Title}");
					email.Text = template.Text;
					email.HtmlText = template.TextHtml;

					foreach(var item in template.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}

					email.Recipient = new EmailContact(clientName, EmailString);
					email.Sender = new EmailContact(organizationName, ParametersProvider.Instance.GetParameterValue("email_for_email_delivery"));
					email.Order = document.Id;
					email.OrderDocumentType = document.Type;

					using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
						string billDate = billWSFPDocument.DocumentDate.HasValue ? "_" + billWSFPDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
						email.AddAttachment($"Bill_{billWSFPDocument.Id}{billDate}.pdf", stream);
					}
					return email;
				default:
					return null;
			} 
		}

		/*
		private void CreateNewEmail<T>(string clientName, string organizationName, T billDocument)
		{
			var wasHideSignature = billDocument.HideSignature;
			billDocument.HideSignature = false;
			ReportInfo ri = billDocument.GetReportInfo();
			billDocument.HideSignature = wasHideSignature;

			EmailTemplate template = billDocument.GetEmailTemplate();
			EmailService.Email email = new EmailService.Email();
			email.Title = string.Format($"{template.Title} {billDocument.Title}");
			email.Text = template.Text;
			email.HtmlText = template.TextHtml;
			foreach(var item in template.Attachments) {
				email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
			}

			email.Recipient = new EmailContact(clientName, EmailString);
			email.Sender = new EmailContact(organizationName, ParametersProvider.Instance.GetParameterValue("email_for_email_delivery"));
			email.Order = document.Order.Id;
			email.OrderDocumentType = document.Type;
			using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
				string billDate = billDocument.DocumentDate.HasValue ? "_" + billDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
				email.AddAttachment($"Bill_{billDocument.Order.Id}{billDate}.pdf", stream);
			}
		}*/
	}
}

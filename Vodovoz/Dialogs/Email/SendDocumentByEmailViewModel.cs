using System.Collections.Generic;
using EmailService;
using QS.DomainModel.UoW;
using QS.ViewModels;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using System.Data.Bindings.Collections.Generic;
using QS.Report;
using Vodovoz.Parameters;
using Vodovoz.EntityRepositories.Employees;
using System.IO;
using fyiReporting.RDL;
using QS.Commands;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog;
using QS.Project.Services;
using QS.Services;
using RdlEngine;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Orders;

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
		private readonly IInteractiveService interactiveService;
		private readonly IParametersProvider _parametersProvider;
		private readonly Employee _currentEmployee;
		private IDocument Document { get; set; }

		public GenericObservableList<StoredEmail> StoredEmails { get; set; }

		public DelegateCommand SendEmailCommand { get; private set; }

		public DelegateCommand RefreshEmailListCommand { get; private set; }

		public SendDocumentByEmailViewModel(
			IEmailRepository emailRepository,
			Employee currentEmployee,
			IInteractiveService interactiveService,
			IParametersProvider parametersProvider,
			IUnitOfWork uow = null)
		{
			this.emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_currentEmployee = currentEmployee;
            this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
            _parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
            StoredEmails = new GenericObservableList<StoredEmail>();
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
				() =>
				{
					switch (Document.Type)
					{
						case OrderDocumentType.Bill:
							SendDocument();
							break;
						case OrderDocumentType.BillWSForDebt:
							var billWSForDebt = Document as OrderWithoutShipmentForDebt;

							if (Validate(billWSForDebt))
								SaveAndSend();

							break;
						case OrderDocumentType.BillWSForPayment:
							var billWSForPayment = Document as OrderWithoutShipmentForPayment;

							if (Validate(billWSForPayment))
								SaveAndSend();

							break;
						case OrderDocumentType.BillWSForAdvancePayment:
							var billWSForAdvancePayment = Document as OrderWithoutShipmentForAdvancePayment;

							if (Validate(billWSForAdvancePayment))
								SaveAndSend();

							break;
					}
				},
				() => !string.IsNullOrEmpty(EmailString)
			);
		}

		private bool Validate<T>(T Entity)
		{
			return ServicesConfig.CommonServices.ValidationService.Validate(Entity, new ValidationContext(Entity));
		}

		private void SaveAndSend()
		{
			UoW.Save();
			SendDocument();
		}

		private void CreateRefreshEmailListCommand()
		{
			RefreshEmailListCommand = new DelegateCommand(
				UpdateEmails,
				() => {
					if(Document == null) {
						return false;
					}
					if (Document.Type == OrderDocumentType.Bill) {
						return Document?.Order != null;
					}
					else {
						return Document?.Id != 0;
					}
				}
			);
		}

		public void Update(IDocument document, string email)
		{
			Document = document;
			
			EmailString = email;

			if(!string.IsNullOrEmpty(EmailString))
				UpdateEmails();
			else
				BtnSendEmailSensitive = false;
		}

		public void UpdateEmails()
		{
			StoredEmails.Clear();
			
			if (Document == null) return;

			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				IList<StoredEmail> listEmails = null;
				switch (Document.Type)
				{
					case OrderDocumentType.Bill :
						listEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.Order.Id == Document.Order.Id)
							.And(x => x.DocumentType == OrderDocumentType.Bill)
							.List();

						BtnSendEmailSensitive = Document.Type == OrderDocumentType.Bill
						                        && emailRepository.CanSendByTimeout(EmailString, Document.Order.Id, Document.Type) 
						                        && Document.Order.Id > 0;
						break;
					case OrderDocumentType.BillWSForDebt:
						listEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForDebt.Id == Document.Id)
							.And(x => x.DocumentType == OrderDocumentType.BillWSForDebt)
							.List();

						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWSForDebt
						                        && emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					case OrderDocumentType.BillWSForAdvancePayment:
						listEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForAdvancePayment.Id == Document.Id)
							.And(x => x.DocumentType == OrderDocumentType.BillWSForAdvancePayment)
							.List();

						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWSForAdvancePayment
						                        && emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					case OrderDocumentType.BillWSForPayment:
						listEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForPayment.Id == Document.Id)
							.And(x => x.DocumentType == OrderDocumentType.BillWSForPayment)
							.List();

						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWSForPayment
						                        && emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					default:
						BtnSendEmailSensitive = false;
						break;
				}
				
				if(listEmails != null && listEmails.Any())
					UpdateSentEmailsList(listEmails);
			}
		}

		private void UpdateSentEmailsList(IList<StoredEmail> listEmails)
		{
			foreach (StoredEmail item in listEmails)
			{
				StoredEmails.Add(item);
			}
		}

		private void SendDocument()
		{
			var client = Document.Order?.Client;
			var rdlDoc = Document as IPrintableRDLDocument;

			if(rdlDoc == null) {
				interactiveService.ShowMessage(ImportanceLevel.Warning,"Невозможно распечатать данный тип документа");
				return;
			}

			if(Document.Type == OrderDocumentType.Bill && (Document.Order?.Id == 0 || Document.Order?.OrderStatus == OrderStatus.NewOrder)) {
				interactiveService.ShowMessage(ImportanceLevel.Warning,"Для отправки необходимо подтвердить заказ."); 
				return;
			}

			if(Document.Type == OrderDocumentType.Bill && client == null) {
				interactiveService.ShowMessage(ImportanceLevel.Warning,"Должен быть выбран клиент в заказе");
				return;
			}

			if(!_parametersProvider.ContainsParameter("email_for_email_delivery"))
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning,"В параметрах базы не определена почта для рассылки");
				return;
			}

			if(string.IsNullOrWhiteSpace(EmailString)) {
				interactiveService.ShowMessage(ImportanceLevel.Warning,"Необходимо ввести адрес электронной почты");
				return;
			}

			EmailService.OrderEmail email = CreateDocumentEmail("", "vodovoz-spb.ru", Document);
			if(email == null) {
				interactiveService.ShowMessage(ImportanceLevel.Warning,"Для данного типа документа не реализовано формирование письма");
				return;
			}
			
			email.AuthorId = _currentEmployee?.Id ?? 0;
			email.ManualSending = true;

			IEmailService service = EmailServiceSetting.GetEmailService();
			if(service == null) {
				return;
			}
			var result = service.SendOrderEmail(email);

			switch (Document.Type)
			{
				case OrderDocumentType.BillWSForDebt:
					var docForDebt = UoW.GetById<OrderWithoutShipmentForDebt>(Document.Id);
					docForDebt.IsBillWithoutShipmentSent = true;
					UoW.Save();
					break;
				case OrderDocumentType.BillWSForAdvancePayment:
					var docForAdvancePayment = UoW.GetById<OrderWithoutShipmentForAdvancePayment>(Document.Id);
					docForAdvancePayment.IsBillWithoutShipmentSent = true;
					UoW.Save();
					break;
				case OrderDocumentType.BillWSForPayment:
					var docForPayment = UoW.GetById<OrderWithoutShipmentForPayment>(Document.Id);
					docForPayment.IsBillWithoutShipmentSent = true;
					UoW.Save();
					break;
			}

			//Если произошла ошибка и письмо не отправлено
			string resultMessage = "";
			if(!result.Item1) {
				resultMessage = "Письмо не было отправлено! Причина:\n";
			}
			interactiveService.ShowMessage(ImportanceLevel.Info,resultMessage + result.Item2);

			UpdateEmails();
		}

		private EmailService.OrderEmail CreateDocumentEmail(string clientName, string organizationName, IDocument document)
		{
			bool wasHideSignature;
			ReportInfo ri = null;
			EmailTemplate template = null;
			EmailService.OrderEmail email = null;

			switch(document.Type) {

				case OrderDocumentType.Bill	:
					var billDocument = document as BillDocument;
					wasHideSignature = billDocument.HideSignature;
					billDocument.HideSignature = false;
					ri = billDocument.GetReportInfo();
					billDocument.HideSignature = wasHideSignature;

					template = billDocument.GetEmailTemplate();
					email = new EmailService.OrderEmail();
					email.Title = string.Format($"{template.Title} {billDocument.Title}");
					email.Text = template.Text;
					email.HtmlText = template.TextHtml;

					foreach(var item in template.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}

					email.Recipient = new EmailContact(clientName, EmailString);
					email.Sender = new EmailContact(organizationName, _parametersProvider.GetParameterValue("email_for_email_delivery"));
					email.Order = document.Order.Id;
					email.OrderDocumentType = document.Type;

					using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
						string billDate = billDocument.DocumentDate.HasValue ? "_" + billDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
						email.AddAttachment($"Bill_{billDocument.Order.Id}{billDate}.pdf", stream);
					}
					return email;
				case OrderDocumentType.BillWSForDebt:
					var billWSFDDocument = document as OrderWithoutShipmentForDebt;
					wasHideSignature = billWSFDDocument.HideSignature;
					billWSFDDocument.HideSignature = false;
					ri = billWSFDDocument.GetReportInfo();
					billWSFDDocument.HideSignature = wasHideSignature;

					template = billWSFDDocument.GetEmailTemplate();
					email = new EmailService.OrderEmail();
					email.Title = string.Format($"{template.Title} {billWSFDDocument.Title}");
					email.Text = template.Text;
					email.HtmlText = template.TextHtml;

					foreach(var item in template.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}

					email.Recipient = new EmailContact(clientName, EmailString);
					email.Sender = new EmailContact(organizationName, _parametersProvider.GetParameterValue("email_for_email_delivery"));
					email.Order = document.Id;
					email.OrderDocumentType = document.Type;

					using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
						string billDate = billWSFDDocument.DocumentDate.HasValue ? "_" + billWSFDDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
						email.AddAttachment($"Bill_{billWSFDDocument.Id}{billDate}.pdf", stream);
					}
					return email;
				case OrderDocumentType.BillWSForAdvancePayment:
					var billWSFAPDocument = document as OrderWithoutShipmentForAdvancePayment;
					wasHideSignature = billWSFAPDocument.HideSignature;
					billWSFAPDocument.HideSignature = false;
					ri = billWSFAPDocument.GetReportInfo();
					billWSFAPDocument.HideSignature = wasHideSignature;

					template = billWSFAPDocument.GetEmailTemplate();
					email = new EmailService.OrderEmail();
					email.Title = string.Format($"{template.Title} {billWSFAPDocument.Title}");
					email.Text = template.Text;
					email.HtmlText = template.TextHtml;

					foreach(var item in template.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}

					email.Recipient = new EmailContact(clientName, EmailString);
					email.Sender = new EmailContact(organizationName, _parametersProvider.GetParameterValue("email_for_email_delivery"));
					email.Order = document.Id;
					email.OrderDocumentType = document.Type;

					using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), ri.ConnectionString, OutputPresentationType.PDF, true)) {
						string billDate = billWSFAPDocument.DocumentDate.HasValue ? "_" + billWSFAPDocument.DocumentDate.Value.ToString("ddMMyyyy") : "";
						email.AddAttachment($"Bill_{billWSFAPDocument.Id}{billDate}.pdf", stream);
					}
					return email;
				case OrderDocumentType.BillWSForPayment:
					var billWSFPDocument = document as OrderWithoutShipmentForPayment;
					wasHideSignature = billWSFPDocument.HideSignature;
					billWSFPDocument.HideSignature = false;
					ri = billWSFPDocument.GetReportInfo();
					billWSFPDocument.HideSignature = wasHideSignature;

					template = billWSFPDocument.GetEmailTemplate();
					email = new EmailService.OrderEmail();
					email.Title = string.Format($"{template.Title} {billWSFPDocument.Title}");
					email.Text = template.Text;
					email.HtmlText = template.TextHtml;

					foreach(var item in template.Attachments) {
						email.AddInlinedAttachment(item.Key, item.Value.MIMEType, item.Value.FileName, item.Value.Base64Content);
					}

					email.Recipient = new EmailContact(clientName, EmailString);
					email.Sender = new EmailContact(organizationName, _parametersProvider.GetParameterValue("email_for_email_delivery"));
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
	}
}

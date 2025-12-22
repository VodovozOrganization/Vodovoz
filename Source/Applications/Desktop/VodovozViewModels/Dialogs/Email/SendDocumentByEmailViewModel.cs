using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;

namespace Vodovoz.ViewModels.Dialogs.Email
{
	public class SendDocumentByEmailViewModel : UoWWidgetViewModelBase
	{
		private string _emailString;
		public string EmailString {
			get => _emailString;
			set => SetField(ref _emailString, value);
		}

		private string _description;
		public string Description {
			get => _description;
			set => SetField(ref _description, value);
		}

		private bool _btnSendEmailSensitive;
		public bool BtnSendEmailSensitive {
			get => _btnSendEmailSensitive;
			set => SetField(ref _btnSendEmailSensitive, value);
		}

		private StoredEmail _selectedStoredEmail;
		public StoredEmail SelectedStoredEmail {
			get => _selectedStoredEmail;
			set => SetField(ref _selectedStoredEmail, value);
		}

		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailSettings _emailSettings;
		private readonly Employee _employee;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;

		private bool _canManuallyResendUpd =>
			_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.Documents.CanManuallyResendUpd);

		private IEmailableDocument Document { get; set; }

		public GenericObservableList<StoredEmail> StoredEmails { get; set; }

		public DelegateCommand SendEmailCommand { get; private set; }

		public DelegateCommand RefreshEmailListCommand { get; private set; }

		public SendDocumentByEmailViewModel(IUnitOfWorkFactory uowFactory, IEmailRepository emailRepository, IEmailSettings emailSettings,
									  Employee employee, ICommonServices commonServices, IUnitOfWork uow = null)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_employee = employee;
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = _commonServices.InteractiveService;
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
						case OrderDocumentType.SpecialBill:
						case OrderDocumentType.UPD:
						case OrderDocumentType.SpecialUPD:
							SendDocument();
							break;
						case OrderDocumentType.BillWSForDebt:
							var billWSForDebt = Document as OrderWithoutShipmentForDebt;
							if(Validate(billWSForDebt))
							{
								SaveAndSend();
							}
							break;
						case OrderDocumentType.BillWSForPayment:
							var billWSForPayment = Document as OrderWithoutShipmentForPayment;
							if(Validate(billWSForPayment))
							{
								SaveAndSend();
							}
							break;
						case OrderDocumentType.BillWSForAdvancePayment:
							var billWSForAdvancePayment = Document as OrderWithoutShipmentForAdvancePayment;
							if (Validate(billWSForAdvancePayment))
							{
								SaveAndSend();
							}
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
					if(Document == null)
					{
						return false;
					}
					if(Document.Type == OrderDocumentType.Bill || Document.Type == OrderDocumentType.SpecialBill)
					{
						return Document?.Order != null;
					}
					
					return Document?.Id != 0;
				}
			);
		}

		public void Update(IEmailableDocument document, string email)
		{
			Document = document;
			
			EmailString = email;

			if(string.IsNullOrEmpty(EmailString) || Document == null)
			{
				BtnSendEmailSensitive = false;
			}

			UpdateEmails();
		}

		public void UpdateEmails()
		{
			StoredEmails.Clear();

			if(Document == null)
			{
				return;
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				IList<StoredEmail> listEmails = null;

				switch(Document.Type)
				{
					case OrderDocumentType.Bill:
					case OrderDocumentType.SpecialBill:
						listEmails = uow.Session.QueryOver<BillDocumentEmail>()
							.Where(o => o.OrderDocument.Id == Document.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, Document.Order.Id, Document.Type) 
												&& Document.Order.Id > 0;
						break;
					case OrderDocumentType.BillWSForDebt:
						listEmails = uow.Session.QueryOver<OrderWithoutShipmentForDebtEmail>()
							.Where(o => o.OrderWithoutShipmentForDebt.Id == Document.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type)
							&& ((OrderWithoutShipmentForDebt)Document).Organization != null;
						break;
					case OrderDocumentType.BillWSForAdvancePayment:
						listEmails = uow.Session.QueryOver<OrderWithoutShipmentForAdvancePaymentEmail>()
							.Where(o => o.OrderWithoutShipmentForAdvancePayment.Id == Document.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type)
						    && ((OrderWithoutShipmentForAdvancePayment)Document).Organization != null;
						break;
					case OrderDocumentType.BillWSForPayment:
						listEmails = uow.Session.QueryOver<OrderWithoutShipmentForPaymentEmail>()
							.Where(o => o.OrderWithoutShipmentForPayment.Id == Document.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type)
						    && ((OrderWithoutShipmentForPayment)Document).Organization != null;
						break;
					case OrderDocumentType.LetterOfDebt:
						listEmails = uow.Session.QueryOver<BulkEmail>()
							.Where(o => o.OrderDocument.Id == Document.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = false;
						break;
					case OrderDocumentType.UPD:
					case OrderDocumentType.SpecialUPD:
						listEmails = uow.Session.QueryOver<UpdDocumentEmail>()
							.Where(o => o.OrderDocument.Id == Document.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive =
							_emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type)
							&& _canManuallyResendUpd;
						break;
					default:
						BtnSendEmailSensitive = false;
						break;
				}
				
				if(listEmails != null && listEmails.Any())
				{
					UpdateSentEmailsList(listEmails);
				}
			}
		}

		private void UpdateSentEmailsList(IList<StoredEmail> listEmails)
		{
			foreach (StoredEmail item in listEmails)
			{
				StoredEmails.Add(item);
			}
		}

		private bool CanSendDocument(Domain.Client.Counterparty client)
		{
			var rdlDoc = Document as IPrintableRDLDocument;

			var result = true;

			var stringBuilder = new StringBuilder();

			if(rdlDoc == null)
			{
				stringBuilder.AppendLine("Невозможно распечатать данный тип документа");
				result = false;
			}

			if((Document.Type == OrderDocumentType.Bill || Document.Type == OrderDocumentType.SpecialBill)
				&& (Document.Order?.Id == 0 || Document.Order?.OrderStatus == OrderStatus.NewOrder))
			{
				stringBuilder.AppendLine("Для отправки необходимо подтвердить заказ.");
				result = false;
			}

			if((Document.Type == OrderDocumentType.Bill || Document.Type == OrderDocumentType.SpecialBill)
			   && client == null)
			{
				stringBuilder.AppendLine("Должен быть выбран клиент в заказе");
				result = false;
			}

			try
			{
				_ = _emailSettings.DocumentEmailSenderAddress;
			}
			catch(InvalidProgramException)
			{
				stringBuilder.AppendLine("В параметрах базы не определена почта для рассылки");
				result = false;
			}

			try
			{
				_ = _emailSettings.DocumentEmailSenderName;
			}
			catch(InvalidProgramException)
			{
				stringBuilder.AppendLine("В параметрах базы не определено имя отправителя");
				result = false;
			}

			if(string.IsNullOrWhiteSpace(EmailString))
			{
				stringBuilder.AppendLine("Необходимо ввести адрес электронной почты");
				result = false;
			}

			if(!result)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, stringBuilder.ToString());
			}

			return result;
		}

		private void SendDocument()
		{
			var client = Document.Counterparty;

			if(!CanSendDocument(client))
			{
				return;
			}

			using(var unitOfWork = _uowFactory.CreateWithoutRoot("StoredEmail"))
			{
				var storedEmail = new StoredEmail
				{
					State = StoredEmailStates.PreparingToSend,
					Author = _employee,
					ManualSending = true,
					SendDate = DateTime.Now,
					StateChangeDate = DateTime.Now,
					Subject = Document.Title,
					RecipientAddress = EmailString
				};

				try
				{
					unitOfWork.Save(storedEmail);
					
					switch(Document.Type)
					{
						case OrderDocumentType.Bill:
						case OrderDocumentType.SpecialBill:
							var orderDocumentEmail = new BillDocumentEmail
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderDocument = (OrderDocument) Document
							};
							unitOfWork.Save(orderDocumentEmail);
							break;
						case OrderDocumentType.BillWSForDebt:
							var docForDebt = UoW.GetById<OrderWithoutShipmentForDebt>(Document.Id);
							docForDebt.IsBillWithoutShipmentSent = true;
							UoW.Save();
							var orderWithoutShipmentForDebtEmail = new OrderWithoutShipmentForDebtEmail()
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderWithoutShipmentForDebt = (OrderWithoutShipmentForDebt) Document
							};
							unitOfWork.Save(orderWithoutShipmentForDebtEmail);
							break;
						case OrderDocumentType.BillWSForAdvancePayment:
							var docForAdvancePayment = UoW.GetById<OrderWithoutShipmentForAdvancePayment>(Document.Id);
							docForAdvancePayment.IsBillWithoutShipmentSent = true;
							UoW.Save();
							var orderWithoutShipmentForAdvancePaymentEmail = new OrderWithoutShipmentForAdvancePaymentEmail()
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderWithoutShipmentForAdvancePayment = (OrderWithoutShipmentForAdvancePayment)Document
							};
							unitOfWork.Save(orderWithoutShipmentForAdvancePaymentEmail);
							break;
						case OrderDocumentType.BillWSForPayment:
							var docForPayment = UoW.GetById<OrderWithoutShipmentForPayment>(Document.Id);
							docForPayment.IsBillWithoutShipmentSent = true;
							UoW.Save();
							var orderWithoutShipmentForPaymentEmail = new OrderWithoutShipmentForPaymentEmail()
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderWithoutShipmentForPayment = (OrderWithoutShipmentForPayment)Document
							};
							unitOfWork.Save(orderWithoutShipmentForPaymentEmail);
							break;
						case OrderDocumentType.UPD:
						case OrderDocumentType.SpecialUPD:
							var updDocumentEmail = new UpdDocumentEmail
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderDocument = (OrderDocument)Document
							};
							unitOfWork.Save(updDocumentEmail);
							break;
					}

					unitOfWork.Commit();
				}
				catch(Exception e)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, e.Message);
				}

				UpdateEmails();
			}
		}
	}
}

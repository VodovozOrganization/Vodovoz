using iTextSharp.text.pdf;
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
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;
using OrderWithoutShipmentForAdvancePayment = Vodovoz.Domain.Orders.OrdersWithoutShipment.OrderWithoutShipmentForAdvancePayment;
using OrderWithoutShipmentForDebt = Vodovoz.Domain.Orders.OrdersWithoutShipment.OrderWithoutShipmentForDebt;
using OrderWithoutShipmentForPayment = Vodovoz.Domain.Orders.OrdersWithoutShipment.OrderWithoutShipmentForPayment;

namespace Vodovoz.ViewModels.Dialogs.Email
{
	public class SendDocumentByEmailViewModel : UoWWidgetViewModelBase
	{
		private string _emailString;
		public string EmailString
		{
			get => _emailString;
			set => SetField(ref _emailString, value);
		}

		private string _description;
		public string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		private bool _btnSendEmailSensitive;
		public bool BtnSendEmailSensitive
		{
			get => _btnSendEmailSensitive;
			set => SetField(ref _btnSendEmailSensitive, value);
		}

		private StoredEmail _selectedStoredEmail;
		public StoredEmail SelectedStoredEmail
		{
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
					switch(Document)
					{
						case BillDocument billDocument:
						case SpecialBillDocument specialBillDocument:
						case UPDDocument updDocument:
						case SpecialUPDDocument specialUpdDocument:
						case EquipmentTransferDocument equipmentTransferDocument:
							SendDocument();
							break;
						case OrderWithoutShipmentForDebt billWSForDebt:
							if(Validate(billWSForDebt))
							{
								SaveAndSend();
							}
							break;
						case OrderWithoutShipmentForPayment billWSForPayment:
							if(Validate(billWSForPayment))
							{
								SaveAndSend();
							}
							break;
						case OrderWithoutShipmentForAdvancePayment billWSForAdvancePayment:
							if(Validate(billWSForAdvancePayment))
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
				() =>
				{
					if(Document == null)
					{
						return false;
					}

					var orderDocument = Document as OrderDocument;

					if(orderDocument is BillDocument || orderDocument is SpecialBillDocument)
					{
						return orderDocument.Order != null;
					}

					return orderDocument?.Id != 0;
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

				var orderDocument = Document as OrderDocument;

				switch(Document)
				{
					case BillDocument billDocument:
					case SpecialBillDocument specialBillDocument:
						listEmails = uow.Session.QueryOver<BillDocumentEmail>()
							.Where(o => o.OrderDocument.Id == orderDocument.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, orderDocument.Order.Id, orderDocument.Type)
												&& orderDocument.Order.Id > 0;
						break;
					case OrderWithoutShipmentForDebt billWSForDebt:
						listEmails = uow.Session.QueryOver<OrderWithoutShipmentForDebtEmail>()
							.Where(o => o.OrderWithoutShipmentForDebt.Id == billWSForDebt.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, billWSForDebt.Id, billWSForDebt.Type)
							&& ((OrderWithoutShipmentForDebt)Document).Organization != null;
						break;
					case OrderWithoutShipmentForAdvancePayment billWSForAdvancePayment:
						listEmails = uow.Session.QueryOver<OrderWithoutShipmentForAdvancePaymentEmail>()
							.Where(o => o.OrderWithoutShipmentForAdvancePayment.Id == billWSForAdvancePayment.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, billWSForAdvancePayment.Id, billWSForAdvancePayment.Type)
							&& ((OrderWithoutShipmentForAdvancePayment)Document).Organization != null;
						break;
					case OrderWithoutShipmentForPayment billWSForPayment:
						listEmails = uow.Session.QueryOver<OrderWithoutShipmentForPaymentEmail>()
							.Where(o => o.OrderWithoutShipmentForPayment.Id == billWSForPayment.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, billWSForPayment.Id, billWSForPayment.Type)
							&& ((OrderWithoutShipmentForPayment)Document).Organization != null;
						break;
					case EquipmentTransferDocument equipmentTransferDocument:
						listEmails = uow.Session.QueryOver<EquipmentTransferDocumentEmail>()
							.Where(o => o.OrderDocument.Id == equipmentTransferDocument.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository
							.CanSendByTimeout(EmailString, equipmentTransferDocument.Id, equipmentTransferDocument.Type)
								&& equipmentTransferDocument.Order.Id > 0;
						break;
					case LetterOfDebtDocument letterOfDebtDocument:
						listEmails = uow.Session.QueryOver<BulkEmail>()
							.Where(o => o.OrderDocument.Id == letterOfDebtDocument.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = false;
						break;
					case UPDDocument uPDDocument:
					case SpecialUPDDocument specialUPDDocument:
						listEmails = uow.Session.QueryOver<UpdDocumentEmail>()
							.Where(o => o.OrderDocument.Id == orderDocument.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive =
							_emailRepository.CanSendByTimeout(EmailString, orderDocument.Id, orderDocument.Type)
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
			foreach(StoredEmail item in listEmails)
			{
				StoredEmails.Add(item);
			}
		}

		private bool CanSendDocument(CounterpartyEntity client)
		{
			var rdlDoc = Document as IPrintableRDLDocument;

			var result = true;

			var stringBuilder = new StringBuilder();

			if(rdlDoc == null)
			{
				stringBuilder.AppendLine("Невозможно распечатать данный тип документа");
				result = false;
			}

			var orderDocument = Document as OrderDocument;

			if(orderDocument != null && (orderDocument.Type == OrderDocumentType.Bill || orderDocument.Type == OrderDocumentType.SpecialBill)
				&& (orderDocument.Order?.Id == 0 || orderDocument.Order?.OrderStatus == OrderStatus.NewOrder))
			{
				stringBuilder.AppendLine("Для отправки необходимо подтвердить заказ.");
				result = false;
			}

			if(orderDocument != null && (orderDocument.Type == OrderDocumentType.Bill || orderDocument.Type == OrderDocumentType.SpecialBill)
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

					switch(Document)
					{
						case BillDocument billDocument:
						case SpecialBillDocument specialBillDocument:
							var orderDocumentEmail = new BillDocumentEmail
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderDocument = new OrderDocumentEntity { Id = Document.DocumentId }
							};
							unitOfWork.Save(orderDocumentEmail);
							break;
						case OrderWithoutShipmentForDebt docForDebt:
							docForDebt.IsBillWithoutShipmentSent = true;
							UoW.Save();
							var orderWithoutShipmentForDebtEmail = new OrderWithoutShipmentForDebtEmail()
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderWithoutShipmentForDebt = docForDebt
							};
							unitOfWork.Save(orderWithoutShipmentForDebtEmail);
							break;
						case OrderWithoutShipmentForAdvancePayment docForAdvancePayment:
							docForAdvancePayment.IsBillWithoutShipmentSent = true;
							UoW.Save();
							var orderWithoutShipmentForAdvancePaymentEmail = new OrderWithoutShipmentForAdvancePaymentEmail()
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderWithoutShipmentForAdvancePayment = docForAdvancePayment
							};
							unitOfWork.Save(orderWithoutShipmentForAdvancePaymentEmail);
							break;
						case OrderWithoutShipmentForPayment docForPayment:
							docForPayment.IsBillWithoutShipmentSent = true;
							UoW.Save();
							var orderWithoutShipmentForPaymentEmail = new OrderWithoutShipmentForPaymentEmail()
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderWithoutShipmentForPayment = docForPayment
							};
							unitOfWork.Save(orderWithoutShipmentForPaymentEmail);
							break;
						case EquipmentTransferDocument equipmentTransferDocument:
							var equipmentTransfertEmail = new EquipmentTransferDocumentEmail
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderDocument = new OrderDocumentEntity { Id = Document.DocumentId }
							};
							unitOfWork.Save(equipmentTransfertEmail);
							break;
						case UPDDocument uPDDocument:
						case SpecialUPDDocument specialUPDDocument:
							var updDocumentEmail = new UpdDocumentEmail
							{
								StoredEmail = storedEmail,
								Counterparty = client,
								OrderDocument = new OrderDocumentEntity { Id = Document.DocumentId }
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

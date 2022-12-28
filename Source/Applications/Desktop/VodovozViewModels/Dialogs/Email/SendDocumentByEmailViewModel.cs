using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;

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

		private object _selectedObj;
		public object SelectedObj {
			get => _selectedObj;
			set => SetField(ref _selectedObj, value);
		}

		private readonly IEmailRepository _emailRepository;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly Employee _employee;
		private readonly IInteractiveService _interactiveService;

		private IEmailableDocument Document { get; set; }

		public GenericObservableList<StoredEmail> StoredEmails { get; set; }

		public DelegateCommand SendEmailCommand { get; private set; }

		public DelegateCommand RefreshEmailListCommand { get; private set; }

		public SendDocumentByEmailViewModel(IEmailRepository emailRepository, IEmailParametersProvider emailParametersProvider,
									  Employee employee, IInteractiveService interactiveService, IUnitOfWork uow = null)
		{
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_employee = employee;
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
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
					else
					{
						return Document?.Id != 0;
					}
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

			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				IList<StoredEmail> listEmails = null;

				switch(Document.Type)
				{
					case OrderDocumentType.Bill:
					case OrderDocumentType.SpecialBill:
						listEmails = uow.Session.QueryOver<OrderDocumentEmail>()
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

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					case OrderDocumentType.BillWSForAdvancePayment:
						listEmails = uow.Session.QueryOver<OrderWithoutShipmentForAdvancePaymentEmail>()
							.Where(o => o.OrderWithoutShipmentForAdvancePayment.Id == Document.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					case OrderDocumentType.BillWSForPayment:
						listEmails = uow.Session.QueryOver<OrderWithoutShipmentForPaymentEmail>()
							.Where(o => o.OrderWithoutShipmentForPayment.Id == Document.Id)
							.Select(o => o.StoredEmail)
							.List<StoredEmail>();

						BtnSendEmailSensitive = _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
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
				_ = _emailParametersProvider.DocumentEmailSenderAddress;
			}
			catch(InvalidProgramException)
			{
				stringBuilder.AppendLine("В параметрах базы не определена почта для рассылки");
				result = false;
			}

			try
			{
				_ = _emailParametersProvider.DocumentEmailSenderName;
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

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("StoredEmail"))
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
							var orderDocumentEmail = new OrderDocumentEmail
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

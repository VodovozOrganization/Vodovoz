using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QS.ViewModels;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;

namespace Vodovoz.Dialogs.Email
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
		private readonly string _mailPrepareQueueId = "MailPrepareQueue"; // Заменить на другое решение

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
					if(Document.Type == OrderDocumentType.Bill)
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

			if(!string.IsNullOrEmpty(EmailString))
			{
				UpdateEmails();
			}
			else
			{
				BtnSendEmailSensitive = false;
			}
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
					case OrderDocumentType.Bill :
						listEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.Order.Id == Document.Order.Id)
							.And(x => x.DocumentType == OrderDocumentType.Bill)
							.List();

						BtnSendEmailSensitive = Document.Type == OrderDocumentType.Bill
												&& _emailRepository.CanSendByTimeout(EmailString, Document.Order.Id, Document.Type) 
												&& Document.Order.Id > 0;
						break;
					case OrderDocumentType.BillWSForDebt:
						listEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForDebt.Id == Document.Id)
							.And(x => x.DocumentType == OrderDocumentType.BillWSForDebt)
							.List();

						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWSForDebt
												&& _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					case OrderDocumentType.BillWSForAdvancePayment:
						listEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForAdvancePayment.Id == Document.Id)
							.And(x => x.DocumentType == OrderDocumentType.BillWSForAdvancePayment)
							.List();

						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWSForAdvancePayment
												&& _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
						break;
					case OrderDocumentType.BillWSForPayment:
						listEmails = uow.Session.QueryOver<StoredEmail>()
							.Where(x => x.OrderWithoutShipmentForPayment.Id == Document.Id)
							.And(x => x.DocumentType == OrderDocumentType.BillWSForPayment)
							.List();

						BtnSendEmailSensitive = Document.Type == OrderDocumentType.BillWSForPayment
												&& _emailRepository.CanSendByTimeout(EmailString, Document.Id, Document.Type);
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

		private bool CanSendDocument(Counterparty client)
		{
			var rdlDoc = Document as IPrintableRDLDocument;

			var result = true;

			var stringBuilder = new StringBuilder();

			if(rdlDoc == null)
			{
				stringBuilder.AppendLine("Невозможно распечатать данный тип документа");
				result = false;
			}

			if(Document.Type == OrderDocumentType.Bill 
				&& (Document.Order?.Id == 0 || Document.Order?.OrderStatus == OrderStatus.NewOrder))
			{
				stringBuilder.AppendLine("Для отправки необходимо подтвердить заказ.");
				result = false;
			}

			if(Document.Type == OrderDocumentType.Bill && client == null)
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

			_interactiveService.ShowMessage(ImportanceLevel.Warning, stringBuilder.ToString());

			return result;
		}

		private void SendDocument()
		{
			var client = Document.Order?.Client;

			if(!CanSendDocument(client))
			{
				return;
			}

			using(var unitOfWork = UnitOfWorkFactory.CreateWithNewRoot<StoredEmail>())
			{
				unitOfWork.Root.Author = _employee;
				unitOfWork.Root.ManualSending = true;
				unitOfWork.Root.DocumentType = Document.Type;
				unitOfWork.Root.Order = Document.Order;
				unitOfWork.Root.SendDate = DateTime.Now;
				unitOfWork.Root.StateChangeDate = DateTime.Now;
				unitOfWork.Root.State = StoredEmailStates.WaitingToSend;
				unitOfWork.Root.RecipientAddress = EmailString;

				unitOfWork.Save();

				var prepareMailMessage = new PrepareEmailMessage
				{
					StoredEmailId = unitOfWork.Root.Id,
					SendAttemptsCount = 5
				};

				var serializedMessage = JsonSerializer.Serialize(prepareMailMessage);
				var preparingBody = Encoding.UTF8.GetBytes(serializedMessage);

				var Logger = new Logger<RabbitMQConnectionFactory>(new NLogLoggerFactory());

				var rabbitHostname = "";
				var rabbitLogin = "";
				var rabbitPassword = "";
				var rabbitVirtualHost = "";

				var connectionFactory = new RabbitMQConnectionFactory(Logger);
				var connection = connectionFactory.CreateConnection(rabbitHostname, rabbitLogin, rabbitPassword, rabbitVirtualHost);
				var channel = connection.CreateModel();

				try
				{
					channel.BasicPublish("", _mailPrepareQueueId, false, null, preparingBody);

					switch(Document.Type)
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

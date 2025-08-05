using Autofac;
using fyiReporting.RDL;
using Mailganer.Api.Client;
using Mailganer.Api.Client.Dto;
using Mailjet.Api.Abstractions;
using MassTransit;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Tdi;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Settings.Common;

namespace Vodovoz.ViewModels.ReportsParameters
{
	public class RevisionReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private ITdiTab _tdiTab;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _sendRevision;
		private bool _sendBillsForNotPaidOrder;
		private bool _sendGeneralBill;
		private bool _reportIsLoaded;
		private bool _counterpartySelected;
		private bool _canRunReport;
		private Counterparty _counterparty;
		private IList<Email> _emails;
		private Email _selectedEmail;

		private readonly IInteractiveService _interactiveService;
		private readonly MailganerClientV2 _mailganerClient;
		private readonly IEmailSettings _emailSettings;
		private readonly IRequestClient<SendEmailMessage> _client;
		private readonly EmailDirectSender _emailDirectSender;

		public RevisionReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IInteractiveService interactiveService,
			MailganerClientV2 mailganerClient,
			IEmailSettings emailSettings,
			IRequestClient<SendEmailMessage> client,
			EmailDirectSender emailDirectSender
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			RdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_mailganerClient = mailganerClient ?? throw new ArgumentNullException(nameof(mailganerClient));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_emailDirectSender = emailDirectSender ?? throw new ArgumentNullException(nameof(emailDirectSender));

			SendByEmailCommand = new DelegateCommand(() => SendByEmail());
			RunCommand = new DelegateCommand(() =>
			{
				this.LoadReport();
				ReportIsLoaded = true;
			});

			Title = "Акт сверки";
			Identifier = "Client.Revision";

			// Убрать
			ReportIsLoaded = true;
		}

		#region Properties
		public DateTime? StartDate
		{
			get => _startDate;
			set
			{
				if(SetField(ref _startDate, value, () => StartDate))
				{
					CanRunReport = CounterpartySelected && value.HasValue && EndDate.HasValue;
					ReportIsLoaded = false;
				}
			}
		}
		public DateTime? EndDate
		{
			get => _endDate;
			set
			{
				if(SetField(ref _endDate, value, () => EndDate))
				{
					CanRunReport = CounterpartySelected && StartDate.HasValue && value.HasValue;
					ReportIsLoaded = false;
				}
			}
		}

		public ITdiTab TdiTab
		{
			get => _tdiTab;
			set => SetField(ref _tdiTab, value);
		}

		public bool SendRevision
		{
			get => _sendRevision;
			set => SetField(ref _sendRevision, value, () => SendRevision);
		}

		public bool SendBillsForNotPaidOrder
		{
			get => _sendBillsForNotPaidOrder;
			set => SetField(ref _sendBillsForNotPaidOrder, value, () => SendBillsForNotPaidOrder);
		}

		public bool SendGeneralBill
		{
			get => _sendGeneralBill;
			set => SetField(ref _sendGeneralBill, value, () => SendGeneralBill);
		}

		public Counterparty Counterparty
		{
			get => _counterparty;
			set
			{
				if(SetField(ref _counterparty, value, () => Counterparty))
				{
					CounterpartySelected = value != null;
					Emails = value?.Emails ?? new List<Email>();
					ReportIsLoaded = false;
				}
			}
		}
		public bool ReportIsLoaded
		{
			get => _reportIsLoaded;
			set => SetField(ref _reportIsLoaded, value, () => ReportIsLoaded);
		}
		public bool CounterpartySelected
		{
			get => _counterpartySelected;
			set
			{
				if(SetField(ref _counterpartySelected, value, () => CounterpartySelected))
				{
					CanRunReport = value && StartDate.HasValue && EndDate.HasValue;
				}
			}
		}
		public bool CanRunReport
		{
			get => _canRunReport;
			set => SetField(ref _canRunReport, value, () => CanRunReport);
		}

		public IList<Email> Emails
		{
			get => _emails;
			set => SetField(ref _emails, value, () => Emails);
		}
		public Email SelectedEmail
		{
			get => _selectedEmail;
			set => SetField(ref _selectedEmail, value, () => SelectedEmail);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ILifetimeScope LifetimeScope { get; private set; }
		public INavigationManager NavigationManager { get; }
		public RdlViewerViewModel RdlViewerViewModel { get; }
		public DelegateCommand SendByEmailCommand { get; }
		public DelegateCommand RunCommand { get; }
		#endregion
		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>
		{
			{ "StartDate", StartDate },
			{ "EndDate", EndDate },
			{ "CounterpartyID", Counterparty?.Id }
		};

		public void Dispose()
		{
			LifetimeScope?.Dispose();
			LifetimeScope = null;
			UnitOfWork?.Dispose();
			UnitOfWork = null;
		}
		private async void SendByEmail()
		{
			/*if(Emails.Count == 0 || Emails == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "У контрагента не указан адрес электронной почты");
				return;
			}

			if(SelectedEmail == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана почта для отправки.");
				return;
			}


			if(!SendRevision && !SendBillsForNotPaidOrder && !SendGeneralBill)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран ни один документ для отправки.");
				return;
			}*/
			var t = ReportInfo.Source;
			var z = GenerateReport();
			var g = z;

			try
			{
				// 1. Генерация документа (пример для PDF)
				//var reportInfo = ReportInfoFactory.CreateReportInfo(Parameters);
				//var documentBytes = ReportExporter.ExportToPdf(reportInfo); // Реализуйте этот метод согласно вашей логике
				// 2. Формирование EmailMessage
				var instanceId = Convert.ToInt32(UnitOfWork.Session
					.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
					.List<object>()
					.FirstOrDefault());
				string messageText = "Test message text for email with attachment.";
				var emailMessage = new SendEmailMessage
				{
					//To = SelectedEmail.Address,
					From = new EmailContact
					{
						Name = _emailSettings.DefaultEmailSenderName,
						Email = _emailSettings.DefaultEmailSenderAddress
					},
					To = new List<EmailContact>
					{
						new EmailContact
						{
							//Name = client != null ? client.FullName : "Уважаемый пользователь",
							Name = "Уважаемый пользователь",
							Email = "work.semen.sd@gmail.com"
						}
					},
					Subject = "Акт сверки",
					TextPart = messageText,
					HTMLPart = messageText,
					Payload = new EmailPayload
					{
						Id = 0,
						Trackable = false,
						InstanceId = instanceId
					},
					Attachments = new[]
					{
						new Mailjet.Api.Abstractions.EmailAttachment
						{
							Filename = "АктСверки.pdf",
							//Base64Content = Convert.ToBase64String(documentBytes)
							Base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("PDF STUB"))
						}
					}
				};

				// 3. Отправка через MailganerClientV2
				await _emailDirectSender.SendAsync(emailMessage);

				_interactiveService.ShowMessage(ImportanceLevel.Info, "Письмо успешно отправлено.");
			}
			catch(Exception ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Ошибка при отправке письма: {ex.Message}");
			}
		}

		private byte[] GenerateReport()
		{
			// Получаем XML отчёта из памяти
			string reportXml = ReportInfo.Source;

			if(string.IsNullOrWhiteSpace(reportXml))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Отсутствует XML отчёта для генерации.");
				return null;
			}

			// Создаём объект Report
			var rdlParser = new RDLParser(reportXml);
			var report = rdlParser.Parse();

			// Устанавливаем параметры (если нужно)
			report.RunGetData(Parameters); // Parameters — ваш словарь параметров

			// Генерируем PDF в память
			using(var msGen = new MemoryStreamGen())
			{
				report.RunRender(msGen, OutputPresentationType.PDF);
				msGen.CloseMainStream();
				var pdfBytes = (msGen.GetStream() as MemoryStream)?.ToArray();

				return pdfBytes;
			}
		}
		/*public async Task<bool> SendCodeToEmail(IUnitOfWork uow, GeneratedSecureCode secureCode)
		{
			var instanceId = Convert.ToInt32(uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			Counterparty client = null;

			if(secureCode.CounterpartyId.HasValue)
			{
				client = uow.GetById<Counterparty>(secureCode.CounterpartyId.Value);
			}

			var sendEmailMessage = new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = _emailSettings.DocumentEmailSenderName,
					Email = _emailSettings.DocumentEmailSenderAddress
				},

				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = client != null ? client.FullName : "Уважаемый пользователь",
						Email = secureCode.Target
					}
				},

				Subject = "Код авторизации",
				HTMLPart = null,
				//HTMLPart = SecureCodeEmailHtmlTemplate.GetTemplate(
				//	secureCode.Code, secureCode.Target, _secureCodeSettings.CodeLifetimeSeconds / 60),
				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false,
					InstanceId = instanceId
				},

				Attachments = new List<Mailjet.Api.Abstractions.EmailAttachment>()
			};

			var response = await _client.GetResponse<SentEmailResponse>(sendEmailMessage);
			return response.Message.Sent;
		}*/
	}
}

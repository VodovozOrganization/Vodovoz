using Autofac;
using fyiReporting.RDL;
using Mailganer.Api.Client;
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
using System.Reflection;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Reports.Editing;
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
		private Dictionary<string, object> _parameters;
		private string _source;
		private IncludeExludeFiltersViewModel _filterViewModel;

		private readonly IInteractiveService _interactiveService;
		private readonly IEmailSettings _emailSettings;
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
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_emailDirectSender = emailDirectSender ?? throw new ArgumentNullException(nameof(emailDirectSender));
			_parameters = new Dictionary<string, object>();

			SendByEmailCommand = new DelegateCommand(() => SendByEmail());
			RunCommand = new DelegateCommand(() =>
			{
				GenerateReport();
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
		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;
		public override ReportInfo ReportInfo
		{
			get
			{
				var reportInfo = base.ReportInfo;
				reportInfo.Source = _source;
				reportInfo.UseUserVariables = true;
				return reportInfo;
			}
		}

		public void Dispose()
		{
			LifetimeScope?.Dispose();
			LifetimeScope = null;
			UnitOfWork?.Dispose();
			UnitOfWork = null;
		}
		private async void SendByEmail()
		{
			if(Emails.Count == 0 || Emails == null)
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
			}
			try
			{
				//var reportPdf = GenerateReport(GetReportSource());
				var reportPdf = GenerateReport(ReportInfo.Source);

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
							Base64Content = Convert.ToBase64String(reportPdf)
						}
					}
				};

				_emailDirectSender.SendAsync(emailMessage);

				_interactiveService.ShowMessage(ImportanceLevel.Info, "Письмо успешно отправлено.");
			}
			catch(Exception ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Ошибка при отправке письма: {ex.Message}");
			}
		}

		private string GetRevisionReportSource()
		{
			var root = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var fileName = "Revision.rdl";
			var path = Path.Combine(root, "Reports", "Client", fileName);

			using(var reportController = new ReportController(path))
			using(var reportStream = new MemoryStream())
			{
				// Если есть модификаторы, добавьте их:
				// reportController.AddModifier(new RevisionReportModifier(...));
				reportController.Modify();
				reportController.Save(reportStream);

				reportStream.Position = 0;
				using(var reader = new StreamReader(reportStream))
				{
					return reader.ReadToEnd();
				}
			}
		}
		private void GenerateReport()
		{
			if(StartDate == null || StartDate == default(DateTime))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
			}

			//_parameters = FilterViewModel.GetReportParametersSet();
			_parameters.Add("StartDate", StartDate);
			_parameters.Add("EndDate", EndDate);
			_parameters.Add("creation_date", DateTime.Now);
			_parameters.Add("CounterpartyID", Counterparty?.Id);

			_source = GetReportSource();

			LoadReport();
		}

		private string GetReportSource()
		{
			var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var fileName = "Revision.rdl";
			var path = Path.Combine(root, "Reports", "Client", fileName);

			return ModifyReport(path);
		}

		private string ModifyReport(string path)
		{

			using(ReportController reportController = new ReportController(path))
			using(var reportStream = new MemoryStream())
			{
				reportController.Modify();
				reportController.Save(reportStream);

				using(var reader = new StreamReader(reportStream))
				{
					reportStream.Position = 0;
					var outputSource = reader.ReadToEnd();
					return outputSource;
				}
			}
		}

		/*private ReportModifierBase GetReportModifier()
		{
			ReportModifierBase result;
			var groupParameters = GetGroupingParameters();
			var modifier = new ProfitabilityDetailReportModifier();
			modifier.Setup(groupParameters.Select(x => (GroupingType)x.Value));
			result = modifier;

			return result;
		}*/

		private byte[] GenerateReport(string reportXml)
		{
			if(string.IsNullOrWhiteSpace(reportXml))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Отсутствует XML отчёта для генерации.");
				return null;
			}

			var rdlParser = new RDLParser(reportXml);
			var report = rdlParser.Parse();

			// Устанавливаем параметры (если нужно)
			var preparedParameters = PrepareReportParameters(Parameters);
			report.RunGetData(preparedParameters);

			using(var msGen = new MemoryStreamGen())
			{
				report.RunRender(msGen, OutputPresentationType.PDF);
				msGen.CloseMainStream();
				var pdfBytes = (msGen.GetStream() as MemoryStream)?.ToArray();

				return pdfBytes;
			}
		}
		private Dictionary<string, object> PrepareReportParameters(Dictionary<string, object> parameters)
		{
			var result = new Dictionary<string, object>();
			foreach(var kvp in parameters)
			{
				if(kvp.Value is DateTime dt)
				{
					// Преобразуем дату в нужный формат
					result[kvp.Key] = dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
				}
				else if(kvp.Value is DateTime ndt)
				{
					result[kvp.Key] = ndt.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
				}
				else
				{
					result[kvp.Key] = kvp.Value;
				}
			}
			return result;
		}
	}
}

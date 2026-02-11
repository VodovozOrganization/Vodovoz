using Autofac;
using fyiReporting.RDL;
using iTextSharp.text.pdf;
using Mailganer.Api.Client;
using Mailjet.Api.Abstractions;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.Report.ViewModels;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Data.Bindings;
using System.IO;
using System.Linq;
using System.Reflection;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Reports.Editing;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Organizations;
using Vodovoz.ViewModels.Organizations;
using StoredEmails = Vodovoz.Domain.StoredEmails;

namespace Vodovoz.ViewModels.ReportsParameters
{
	[ToDo ("В RDL файле Revision.rdl используется VB код в секции <Code>, который нужно выпилить." +
		"Функция NUMBER_TO_STRING_DESCRIPTION() не может быть использована в Textbox'е.")]
	public class RevisionReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private readonly int _defaultOurOrganizationId;
		private ITdiTab _tdiTab;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _sendRevision;
		private bool _sendBillsForNotPaidOrder;
		private bool _sendGeneralBill;
		private bool _reportIsLoaded;
		private bool _counterpartySelected;
		private Counterparty _counterparty;
		private Organization _organization;
		private IList<Email> _emails;
		private Email _selectedEmail;
		private string _source;

		private readonly IInteractiveService _interactiveService;
		private readonly IEmailSettings _emailSettings;
		private readonly EmailDirectSender _emailDirectSender;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly Employee _author;

		public RevisionReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IInteractiveService interactiveService,
			IEmailSettings emailSettings,
			EmailDirectSender emailDirectSender,
			IOrderRepository orderRepository,
			IOrganizationRepository organizationRepository,
			IOrganizationSettings organizationSettings,
			IEmployeeRepository employeeRepository
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			RdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_emailDirectSender = emailDirectSender ?? throw new ArgumentNullException(nameof(emailDirectSender));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_author = (employeeRepository ?? throw new ArgumentNullException(nameof(organizationSettings)))
				.GetEmployeeForCurrentUser(UnitOfWork);

			OrganizationViewModel = new CommonEEVMBuilderFactory<RevisionReportViewModel>(
				RdlViewerViewModel,
				this,
				UnitOfWork,
				NavigationManager,
				LifetimeScope)
			.ForProperty(x => x.Organization)
			.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
			.Finish();

			SendByEmailCommand = new DelegateCommand(SendByEmail, () => ReportIsLoaded);
			SendByEmailCommand.CanExecuteChangedWith(this, vm => vm.ReportIsLoaded);
			ShowInfoCommand = new DelegateCommand(() => ShowInfo());
			RunCommand = new DelegateCommand(() =>
			{
				GenerateReport();
				ReportIsLoaded = true;
			},
			() => CanRunReport);
			RunCommand.CanExecuteChangedWith(this, vm => vm.CanRunReport);

			_defaultOurOrganizationId = _organizationSettings.GetCashlessOrganisationId;
			Organization = _organizationRepository.GetOrganizationById(UnitOfWork, _defaultOurOrganizationId);

			Title = "Акт сверки";
			Identifier = "Client.Revision";
		}

		#region Properties
		public DateTime? StartDate
		{
			get => _startDate;
			set
			{
				if(SetField(ref _startDate, value))
				{
					ReportIsLoaded = false;
					OnPropertyChanged(nameof(CanRunReport));
				}
			}
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set
			{
				if(SetField(ref _endDate, value))
				{
					ReportIsLoaded = false;
					OnPropertyChanged(nameof(CanRunReport));
				}
			}
		}

		public ITdiTab TdiTab
		{
			get => _tdiTab;
			set => SetField(ref _tdiTab, value);
		}

		public bool IsSendRevision
		{
			get => _sendRevision;
			set => SetField(ref _sendRevision, value);
		}

		public bool IsSendBillsForNotPaidOrder
		{
			get => _sendBillsForNotPaidOrder;
			set
			{
				if(SetField(ref _sendBillsForNotPaidOrder, value))
				{
					if(value)
					{
						IsSendGeneralBill = false;
					}
				}
			}
		}

		public bool IsSendGeneralBill
		{
			get => _sendGeneralBill;
			set
			{
				if(SetField(ref _sendGeneralBill, value))
				{
					if(value)
					{
						IsSendBillsForNotPaidOrder = false;
					}
				}
			}
		}

		public Counterparty Counterparty
		{
			get => _counterparty;
			set
			{
				if(SetField(ref _counterparty, value))
				{
					CounterpartyIsSelected = value != null;
					Emails = value?.Emails ?? new List<Email>();
					ReportIsLoaded = false;
					OnPropertyChanged(nameof(CanRunReport));
				}
			}
		}

		public Organization Organization
		{
			get => _organization;
			set
			{ 
				if(SetField(ref _organization, value))
				{
					OrganizationIsSelected = value != null;
					ReportIsLoaded = false;
					OnPropertyChanged(nameof(CanRunReport));
				}
			}
		}

		public bool ReportIsLoaded
		{
			get => _reportIsLoaded;
			set => SetField(ref _reportIsLoaded, value);
		}

		public bool CounterpartyIsSelected
		{
			get => _counterpartySelected;
			set
			{
				if(SetField(ref _counterpartySelected, value))
				{
					OnPropertyChanged(nameof(CanRunReport));
				}
			}
		}

		public bool OrganizationIsSelected
		{
			get => _counterpartySelected;
			set
			{
				if(SetField(ref _counterpartySelected, value))
				{
					OnPropertyChanged(nameof(CanRunReport));
				}
			}
		}

		public bool CanRunReport =>
			OrganizationIsSelected
			&& CounterpartyIsSelected
			&& StartDate.HasValue
			&& EndDate.HasValue;

		public IList<Email> Emails
		{
			get => _emails;
			set => SetField(ref _emails, value);
		}

		public Email SelectedEmail
		{
			get => _selectedEmail;
			set => SetField(ref _selectedEmail, value);
		}

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

		public IUnitOfWork UnitOfWork { get; private set; }
		public ILifetimeScope LifetimeScope { get; private set; }
		public IEntityEntryViewModel OrganizationViewModel { get; }
		public INavigationManager NavigationManager { get; }
		public RdlViewerViewModel RdlViewerViewModel { get; }
		public DelegateCommand SendByEmailCommand { get; }
		public DelegateCommand ShowInfoCommand { get; }
		public DelegateCommand RunCommand { get; }

		#endregion
		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>
		{
			{ "StartDate", StartDate },
			{ "EndDate", EndDate },
			{ "CounterpartyId", Counterparty?.Id },
			{ "OrganizationId", Organization?.Id }
		};

		public void Dispose()
		{
			LifetimeScope?.Dispose();
			LifetimeScope = null;
			UnitOfWork?.Dispose();
			UnitOfWork = null;
		}

		private void SendByEmail()
		{
			if(!IsEmailDataValid())
			{
				return;
			}

			var attachments = GetSelectedAttachments();

			if (attachments.Count == 0)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нет документов для отправки.");
				return;
			}

			var email = !string.IsNullOrEmpty(Organization.EmailForMailing) 
				? Organization.EmailForMailing 
				: Organization.Email;

			string messageText = string.IsNullOrEmpty(Organization.EmailForMailing) || 
									string.Equals(email, Organization.Email, StringComparison.OrdinalIgnoreCase) 
				? "Акт сверки."
				: $"Пожалуйста, не отвечайте на это письмо. Для ответа используйте адрес: {Organization.Email}.";

			var subject = "Акт сверки";

			var instanceId = Convert.ToInt32(UnitOfWork.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			var storedEmail = new StoredEmails.StoredEmail
			{
				State = StoredEmails.StoredEmailStates.PreparingToSend,
				Author = _author,
				ManualSending = true,
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				Subject = subject,
				RecipientAddress = SelectedEmail.Address,
				Guid = Guid.NewGuid()
			};

			UnitOfWork.Save(storedEmail);

			var emailMessage = new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = Organization.FullName,
					Email = email
				},
				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = SelectedEmail != null ? SelectedEmail.Counterparty.FullName : "Уважаемый пользователь",
						Email = SelectedEmail.Address
					}
				},
				Subject = subject,
				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = storedEmail.Id,
					Trackable = false,
					InstanceId = instanceId
				},
				Attachments = attachments
			};

			try
			{
				_emailDirectSender.SendAsync(emailMessage).GetAwaiter().GetResult();
				storedEmail.State = StoredEmails.StoredEmailStates.SendingComplete;
				_interactiveService.ShowMessage(ImportanceLevel.Info, "Письмо успешно отправлено.");
			}
			catch(Exception ex)
			{
				storedEmail.State = StoredEmails.StoredEmailStates.SendingError;
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Ошибка при отправке письма: {ex.Message}");
			}
			finally
			{
				UnitOfWork.Save(storedEmail);
				UnitOfWork.Commit();
			}
		}

		private ICollection<EmailAttachment> GetSelectedAttachments()
		{
			var attachments = new List<EmailAttachment>();

			if(IsSendRevision)
			{
				var reportPdf = GenerateReport(ReportInfo.Source, PrepareReportParameters(Parameters));
				if(reportPdf != null)
				{
					attachments.Add(new EmailAttachment
					{
						Filename = "АктСверки.pdf",
						Base64Content = Convert.ToBase64String(reportPdf)
					});
				}
			}

			if(IsSendBillsForNotPaidOrder)
			{
				var unpaidOrdersId = _orderRepository.GetUnpaidOrdersIds(UnitOfWork, Counterparty.Id, StartDate, EndDate, Organization.Id);

				if(unpaidOrdersId.Count == 0)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нет неоплаченных заказов для формирования счетов.");
					return attachments;
				}

				var pdfArray = new byte[unpaidOrdersId.Count][];

				for(int i = 0; i < unpaidOrdersId.Count; i++)
				{
					var billParameters = new Dictionary<string, object>
					{
						{ "order_id", unpaidOrdersId[i] },
						{ "hide_signature", false },
						{ "organization_id", Organization?.Id }
					};
					string billReportSource = GetReportFromDocumentsSource("Bill.rdl");
					byte[] billPdf = GenerateReport(billReportSource, billParameters);
					pdfArray[i] = billPdf;
				}

				byte[] mergedPdf = MergePdfs(pdfArray);

				attachments.Add(new EmailAttachment
				{
					Filename = "Неоплаченные_счета.pdf",
					Base64Content = Convert.ToBase64String(mergedPdf)
				});
			}

			if(IsSendGeneralBill)
			{
				var unpaidOrdersId = _orderRepository.GetUnpaidOrdersIds(UnitOfWork, Counterparty.Id, StartDate, EndDate, Organization.Id);

				if (unpaidOrdersId.Count == 0)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нет заказов для формирования общего счета.");
					return attachments;
				}

				var generalBillParameters = new Dictionary<string, object>
				{
					{ "order_id", unpaidOrdersId },
					{ "hide_signature", false },
					{ "organization_id", Organization ?.Id }
				};
				var generalReportSource = GetReportFromDocumentsSource("GeneralBill.rdl");
				var generalBillPdf = GenerateReport(generalReportSource, generalBillParameters);
				if(generalBillPdf != null)
				{
					attachments.Add(new EmailAttachment
					{
						Filename = "Общий_счет.pdf",
						Base64Content = Convert.ToBase64String(generalBillPdf)
					});
				}
			}

			return attachments;
		}

		private bool IsEmailDataValid()
		{
			if(Emails.Count == 0 || Emails == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "У контрагента не указан адрес электронной почты");
				return false;
			}

			if(string.IsNullOrWhiteSpace(Organization.Email))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Не удалось отправить. Необходимо заполнить E-mail и E-mail для расслыки в карточке организации.");
				return false;
			}

			if(!string.IsNullOrWhiteSpace(Organization.Email) 
				&& !Organization.Email.EndsWith("@vodovoz-spb.ru")
				&& string.IsNullOrWhiteSpace(Organization.EmailForMailing))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Адрес электронной почты организации должен быть в домене @vodovoz-spb.ru");
				return false;
			}

			if(SelectedEmail == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана почта для отправки.");
				return false;
			}

			if(!IsSendRevision && !IsSendBillsForNotPaidOrder && !IsSendGeneralBill)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран ни один документ для отправки.");
				return false;
			}
			return true;
		}

		private void GenerateReport()
		{
			if(StartDate == null || StartDate == default(DateTime))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
				return;
			}

			if(Counterparty == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран контрагент");
				return;
			}

			if(Organization == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбрана организация");
				return;
			}

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

		private string GetReportFromDocumentsSource(string reportFileName)
		{
			var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var path = Path.Combine(root, "Reports", "Documents", reportFileName);

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

		private byte[] GenerateReport(string reportXml, Dictionary<string, object> parms)
		{
			if(string.IsNullOrWhiteSpace(reportXml))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Отсутствует XML отчёта для генерации.");
				return null;
			}

			var rdlParser = new RDLParser(reportXml)
			{
				OverwriteConnectionString = ReportInfo.ConnectionString,
				OverwriteInSubreport = true
			};

			var report = rdlParser.Parse();
			report.RunGetData(parms);

			using(var msGen = new MemoryStreamGen())
			{
				report.RunRender(msGen, OutputPresentationType.PDF);
				msGen.CloseMainStream();
				var pdfBytes = (msGen.GetStream() as MemoryStream)?.ToArray();

				return pdfBytes;
			}
		}

		public byte[] MergePdfs(IEnumerable<byte[]> pdfs)
		{
			using(var mergedPdf = new MemoryStream())
			{
				using(var document = new iTextSharp.text.Document())
				{
					using(var copy = new PdfSmartCopy(document, mergedPdf))
					{
						document.Open();

						foreach(var pdfBytes in pdfs)
						{
							using(var reader = new PdfReader(pdfBytes))
							{
								for(int i = 1; i <= reader.NumberOfPages; i++)
								{
									copy.AddPage(copy.GetImportedPage(reader, i));
								}
							}
						}
					}
				}
				return mergedPdf.ToArray();
			}
		}

		private Dictionary<string, object> PrepareReportParameters(Dictionary<string, object> parameters)
		{
			var result = new Dictionary<string, object>();
			foreach(var kvp in parameters)
			{
				if(kvp.Value is DateTime dt)
				{
					result[kvp.Key] = dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
				}
				else
				{
					result[kvp.Key] = kvp.Value;
				}
			}
			return result;
		}
		private void ShowInfo()
		{
			var info = "В акте сверки заказы, в статусах после \"доставлен\"( включительно), выделяются следующими цветами:"
				+ "\n\t- Желтым цветом — заказы с частичной оплатой;"
				+ "\n\t- Красным цветом — заказы без оплаты.";
			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчётом");
		}
	}
}

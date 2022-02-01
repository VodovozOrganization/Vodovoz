using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using NHibernate;

using QS.Attachments.Domain;
using QS.Attachments.ViewModels.Widgets;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web;
using NLog.Extensions.Logging;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalNodes;
using VodovozInfrastructure.Configuration;

namespace Vodovoz.ViewModels.ViewModels
{
	public class BulkEmailViewModel : DialogViewModelBase
	{
		private readonly IUnitOfWork _uow;
		private string _mailSubject;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly IInteractiveService _interactiveService;
		private readonly IQueryOver<Order> _itemsSourceQuery;
		private readonly int _instanceId;
		private readonly InstanceMailingConfiguration _configuration;
		private DelegateCommand _startEmailSendingCommand;
		private double _sendingProgressValue;
		private double _sendingProgressUpper;
		private IList<Attachment> _attachments = new List<Attachment>();
		private GenericObservableList<Attachment> _observableAttachments;

		public BulkEmailViewModel(INavigationManager navigation, IUnitOfWorkFactory unitOfWorkFactory,
			Func<IUnitOfWork, IQueryOver<Order>> itemsSourceQueryFunction, IEmailParametersProvider emailParametersProvider, IInteractiveService interactiveService,
			IAttachmentsViewModelFactory attachmentsViewModelFactory) : base(navigation)
		{
			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot();
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_itemsSourceQuery = itemsSourceQueryFunction.Invoke(_uow);

			_instanceId = Convert.ToInt32(_uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			_configuration = _uow.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

			AttachmentsEmailViewModel = attachmentsViewModelFactory.CreateNewAttachmentsViewModel(ObservableAttachments);

			SendingProgressUpper = 0;
		}

		private Email SelectPriorityEmail(IList<Email> counterpartyEmails)
		{
			Email email = null;

			email = counterpartyEmails.FirstOrDefault(e => e.EmailType?.Name == "Для счетов")
					?? counterpartyEmails.FirstOrDefault(e => e.EmailType?.Name == "Рабочий")
					?? counterpartyEmails.FirstOrDefault(e => e.EmailType?.Name == "Личный")
					?? counterpartyEmails.FirstOrDefault(e => e.EmailType?.Name == "Для чеков");

			return email;
		}


		#region Commands

		public DelegateCommand StartEmailSendingCommand =>
			_startEmailSendingCommand ?? (_startEmailSendingCommand = new DelegateCommand(() =>
				{
					IsInSendingProcess = true;

					var debtorJournalNodes = _itemsSourceQuery.List<DebtorJournalNode>();

					var counterparties = _uow.GetById<Domain.Client.Counterparty>(debtorJournalNodes.Select(c => c.ClientId));

					SendingProgressUpper = counterparties.Count;
					SendingProgressValue = 0;
					string withoutEmails = string.Empty;

					foreach(var counterparty in counterparties)
					{

						var email = SelectPriorityEmail(counterparty.Emails);
						if(email == null)
						{
							withoutEmails += counterparty.FullName + "; ";
						}
						else
						{
							//SendEmail(email.Address, counterparty.FullName);

						}
						System.Threading.Thread.Sleep(2000);
						SendingProgressValue += 1;
						SendedCountInfo = $"Обработано { SendingProgressValue } из { SendingProgressUpper } контрагентов";
						SendingProgressBarUpdated?.Invoke(this, EventArgs.Empty);
					}

					if(withoutEmails.Length > 0)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning, $"У следующих контрагентов отсутствует email:{ withoutEmails }");
					}

					IsInSendingProcess = false;
				},
				() => true)
			);

		#endregion


		private void SendEmail(string email, string name)
		{
			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = _emailParametersProvider.DocumentEmailSenderName,
					Email = _emailParametersProvider.DocumentEmailSenderAddress
				},

				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = name,
						Email = "9artbe@gmail.com"//email
					}
				},

				Subject = MailSubject,

				TextPart = MailTextPart,
				HTMLPart = MailTextPart,
				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false,
					InstanceId = _instanceId
				}

			};

			var emailAttachments = new List<EmailAttachment>();


			foreach(var attachment in ObservableAttachments)
			{
				emailAttachments.Add(new EmailAttachment
				{
					ContentType = MimeMapping.GetMimeMapping(attachment.FileName),
					Filename = attachment.FileName,
					Base64Content = Convert.ToBase64String(attachment.ByteFile)
				});
			}

			sendEmailMessage.Attachments = emailAttachments;

			try
			{
				var serializedMessage = JsonSerializer.Serialize(sendEmailMessage);
				var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

				var Logger = new Logger<RabbitMQConnectionFactory>(new NLogLoggerFactory());

				var connectionFactory = new RabbitMQConnectionFactory(Logger);
				var connection = connectionFactory.CreateConnection(_configuration.MessageBrokerHost, _configuration.MessageBrokerUsername,
					_configuration.MessageBrokerPassword, _configuration.MessageBrokerVirtualHost);
				var channel = connection.CreateModel();

				var properties = channel.CreateBasicProperties();
				properties.Persistent = true;

				channel.BasicPublish(_configuration.EmailSendExchange, _configuration.EmailSendKey, false, properties, sendingBody);

			}
			finally
			{
				
			}
		}

		[PropertyChangedAlso(nameof(MailSubjectInfo))]
		public string MailSubject
		{
			get => _mailSubject;
			set => SetField(ref _mailSubject, value);
	}

	public string MailSubjectInfo => $"{ MailSubject?.Length ?? 0 }/255 символов";
		public string MailTextPart { get; set; }

		public IList<Attachment> Attachments
		{
			get => _attachments;
			set => SetField(ref _attachments, value);
		}

		public GenericObservableList<Attachment> ObservableAttachments =>
				_observableAttachments ?? (_observableAttachments = new GenericObservableList<Attachment>(Attachments));

		public double SendingProgressValue { get; set; }
		//{
		//	get => _sendingProgressValue;
		//	set => SetField(ref _sendingProgressValue, value);
		//	//FirePropertyChanged();
		//	//OnPropertyChanged();
		//}

		public double SendingProgressUpper { get; set; }
		//{
		//	get => _sendingProgressUpper;
		//	set => SetField(ref _sendingProgressUpper, value);
		//}

		public string SendedCountInfo { get; set; }
		public AttachmentsViewModel AttachmentsEmailViewModel { get; set; }

		public bool IsInSendingProcess
		{
			get => _isInSendingProcess;
			private set => SetField(ref _isInSendingProcess, value);
		}

		public EventHandler SendingProgressBarUpdated;
		private bool _isInSendingProcess;
	}

}

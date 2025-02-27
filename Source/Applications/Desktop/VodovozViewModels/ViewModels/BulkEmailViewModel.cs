using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Dialog;
using RabbitMQ.Client;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Factories;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Settings.Common;
using Vodovoz.ViewModels.Journals.JournalNodes;
using VodovozInfrastructure.Configuration;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels
{
	public class BulkEmailViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<BulkEmailViewModel> _logger;
		private readonly ILogger<RabbitMQConnectionFactory> _rabbitConnectionFactoryLogger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmailSettings _emailSettings;
		private readonly ICommonServices _commonServices;
		private readonly int _instanceId;
		private readonly InstanceMailingConfiguration _configuration;
		private DelegateCommand _startEmailSendingCommand;
		private readonly IObservableList<DetachedFileInformation> _attachments = new ObservableList<DetachedFileInformation>();
		private bool _isInSendingProcess;
		private readonly Employee _author;
		private float _attachmentsSize;
		private string _mailSubject;
		private string _mailTextPart;
		private double _sendingProgressValue;
		private IList<int> _alreadySentCounterpartyIds;
		private int _alreadySentMonthCount;
		private IList<Domain.Client.Counterparty> _counterpartiesToSent;
		private readonly IList<DebtorJournalNode> _debtorJournalNodes;
		private readonly IEmailRepository _emailRepository;
		private bool _canSend = true;
		private int _monthsSinceUnsubscribing;
		private bool _includeOldUnsubscribed;
		private IModel _rabbitMQChannel;
		private IBasicProperties _rabbitMQChannelProperties;

		public BulkEmailViewModel(
			ILogger<BulkEmailViewModel> logger,
			ILogger<RabbitMQConnectionFactory> rabbitConnectionFactoryLogger,
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			Func<IUnitOfWork, IQueryOver<Order>> itemsSourceQueryFunction,
			IEmailSettings emailSettings,
			ICommonServices commonServices,
			IAttachmentsViewModelFactory attachmentsViewModelFactory,
			Employee author,
			IEmailRepository emailRepository,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory) : base(navigation)
		{
			if(attachedFileInformationsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_rabbitConnectionFactoryLogger = rabbitConnectionFactoryLogger ?? throw new ArgumentNullException(nameof(rabbitConnectionFactoryLogger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_uow = _unitOfWorkFactory.CreateWithoutRoot();
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_author = author ?? throw new ArgumentNullException(nameof(author));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_instanceId = emailRepository.GetCurrentDatabaseId(_uow);

			_configuration = _uow.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

			var itemsSourceQuery = itemsSourceQueryFunction.Invoke(_uow);
			_debtorJournalNodes = itemsSourceQuery.List<DebtorJournalNode>();

			MailSubject = string.Empty;

			Init();

			CreateRabbitMQChannel();

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory.Create(_uow, OnAttachmentsAdded, OnAttachmentDeleted, _attachments);
			AttachedFileInformationsViewModel.FileInformations = _attachments;
		}

		private void Init()
		{
			_alreadySentMonthCount = _uow.Session.QueryOver<BulkEmail>()
				.JoinQueryOver(be => be.StoredEmail)
				.Where(se => se.SendDate > DateTime.Now.AddMonths(-1))
				.RowCount();

			var debtorIds = _debtorJournalNodes.Select(dbj => dbj.ClientId).ToArray();

			_alreadySentCounterpartyIds = _uow.Session.QueryOver<BulkEmail>()
				.Where(be => be.Counterparty.Id.IsIn(debtorIds))
				.JoinQueryOver(be => be.StoredEmail)
				.Where(se => se.SendDate > DateTime.Now.AddHours(-2))
				.And(se => se.State != StoredEmailStates.SendingError)
				.Select(be => be.Counterparty.Id)
				.List<int>();

			var notAlreadySendedCounterpartyIds = _debtorJournalNodes
				.Where(d => !_alreadySentCounterpartyIds.Contains(d.ClientId))
				.Select(d => d.ClientId)
				.ToArray();

			Domain.Client.Counterparty counterpartyAlias = null;
			BulkEmailEvent bulkEmailEventAlias = null;

			var query = _uow.Session.QueryOver(() => counterpartyAlias)
				.WhereRestrictionOn(x => x.Id).IsIn(notAlreadySendedCounterpartyIds);

			var lastBulkEmailEventTypeSubquery = QueryOver.Of<BulkEmailEvent>(() => bulkEmailEventAlias)
				.Where(() => bulkEmailEventAlias.Counterparty.Id == counterpartyAlias.Id)
				.OrderBy(x => x.ActionTime).Desc
				.Select(Projections.Property<BulkEmailEvent>(p => p.Type))
				.Take(1);

			if(IncludeOldUnsubscribed)
			{
				var lastBulkEmailEventActionTimeSubquery = QueryOver.Of<BulkEmailEvent>(() => bulkEmailEventAlias)
					.Where(() => bulkEmailEventAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => bulkEmailEventAlias.Type == BulkEmailEvent.BulkEmailEventType.Unsubscribing)
					.OrderBy(x => x.ActionTime).Desc
					.Select(Projections.Property<BulkEmailEvent>(p => p.ActionTime))
					.Take(1);

				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.IsNull(Projections.SubQuery(lastBulkEmailEventTypeSubquery)))
					.Add(Restrictions.Not(Restrictions.Eq(Projections.SubQuery(lastBulkEmailEventTypeSubquery),
						BulkEmailEvent.BulkEmailEventType.Unsubscribing.ToString())))
					.Add(Restrictions.Ge(Projections.SubQuery(lastBulkEmailEventActionTimeSubquery),
						DateTime.Today.AddMonths(-MonthsSinceUnsubscribing))));
			}
			else
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.IsNull(Projections.SubQuery(lastBulkEmailEventTypeSubquery)))
					.Add(Restrictions.Not(Restrictions.Eq(Projections.SubQuery(lastBulkEmailEventTypeSubquery),
					BulkEmailEvent.BulkEmailEventType.Unsubscribing.ToString()))));
			}

			_counterpartiesToSent = query.List();

			SendingProgressValue = 0;

			OnPropertyChanged(nameof(RecepientInfoDanger));
			OnPropertyChanged(nameof(SendingDurationInfo));
		}

		private void CreateRabbitMQChannel()
		{
			var connectionFactory = new RabbitMQConnectionFactory(_rabbitConnectionFactoryLogger);
			var connection = connectionFactory.CreateConnection(_configuration.MessageBrokerHost, _configuration.MessageBrokerUsername,
				_configuration.MessageBrokerPassword, _configuration.MessageBrokerVirtualHost, _configuration.Port, true);
			_rabbitMQChannel = connection.CreateModel();
			_rabbitMQChannelProperties = _rabbitMQChannel.CreateBasicProperties();
			_rabbitMQChannelProperties.Persistent = true;
		}

		private void OnAttachmentsAdded(string fileName)
		{
			if(!_commonServices.InteractiveService.Question(
				$"Использование вложений повышает вероятность попадания в спам. Лучше передать информацию в тексте письма.\nВы точно хотите использовать вложения?"))
			{
				return;
			}
			else
			{
				if(_attachments.Any(afi => afi.FileName == fileName))
				{
					return;
				}

				_attachments.Add(new DetachedFileInformation
				{
					FileName = fileName,
				});

				RecalculateSize();
			}
		}

		private void OnAttachmentDeleted(string fileName)
		{
			_attachments.Remove(_attachments.FirstOrDefault(a => a.FileName == fileName));
			RecalculateSize();
		}

		private void RecalculateSize()
		{
			_attachmentsSize = 0;

			foreach(var attachment in AttachedFileInformationsViewModel.AttachedFiles.Where(af => _attachments.Any(a => a.FileName == af.Key)))
			{
				_attachmentsSize += (attachment.Value.Length / 1024f) / 1024f;
			}

			OnPropertyChanged(nameof(AttachmentsSizeInfoDanger));
		}

		private Email SelectPriorityEmail(IList<Email> counterpartyEmails)
		{
			var email = counterpartyEmails.FirstOrDefault(e => e.EmailType?.EmailPurpose == EmailPurpose.ForBills)
						?? counterpartyEmails.FirstOrDefault(e => e.EmailType?.EmailPurpose == EmailPurpose.Work)
						?? counterpartyEmails.FirstOrDefault(e => e.EmailType?.EmailPurpose == EmailPurpose.Personal)
						?? counterpartyEmails.FirstOrDefault(e => e.EmailType?.EmailPurpose == EmailPurpose.ForReceipts)
						?? counterpartyEmails.FirstOrDefault();

			return email;
		}

		private void SendEmail(string email, string name, StoredEmail storedEmail)
		{
			var sendEmailMessage = new SendEmailMessage()
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
						Name = name,
						Email = email
					}
				},

				Subject = MailSubject,
				TextPart = MailTextPart,
				HTMLPart = $"{MailTextPart}\n\n{GetUnsubscribeHtmlPart(storedEmail.Guid.Value)}",
				Payload = new EmailPayload
				{
					Id = storedEmail.Id,
					Trackable = true,
					InstanceId = _instanceId
				},
				Headers = new Dictionary<string, string>
				{
					{ "List-Unsubscribe" , $"{GetUnsubscribeLink(storedEmail.Guid.Value)}" }
				}
			};

			var emailAttachments = new List<EmailAttachment>();

			foreach(var keyValuePair in AttachedFileInformationsViewModel.AttachedFiles.Where(af => _attachments.Any(a => a.FileName == af.Key)))
			{
				emailAttachments.Add(new EmailAttachment
				{
					ContentType = MimeMapping.GetMimeMapping(keyValuePair.Key),
					Filename = keyValuePair.Key,
					Base64Content = Convert.ToBase64String(keyValuePair.Value)
				});
			}

			sendEmailMessage.Attachments = emailAttachments;

			var serializedMessage = JsonSerializer.Serialize(sendEmailMessage);
			var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

			_rabbitMQChannel.BasicPublish(_configuration.EmailSendExchange, _configuration.EmailSendKey, false, _rabbitMQChannelProperties, sendingBody);
		}

		private string GetUnsubscribeHtmlPart(Guid guid) => $"<br/><br/><a href=\"{GetUnsubscribeLink(guid)}\">Отписаться от рассылки</a>";

		private string GetUnsubscribeLink(Guid guid) => $"{_emailSettings.UnsubscribeUrl}/{guid}";

		#region Commands

		public DelegateCommand StartEmailSendingCommand =>
			_startEmailSendingCommand ?? (_startEmailSendingCommand = new DelegateCommand(() =>
			{
				if(!_commonServices.InteractiveService.Question($"Отправить письмо {_counterpartiesToSent.Count} выбранным клиентам?"))
				{
					return;
				}

				IsInSendingProcess = true;

				string withoutEmails = string.Empty;
				string sendingErrors = string.Empty;

				OnPropertyChanged(nameof(SendingProgressUpper));

				using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("BulkEmail"))
				{
					foreach(var counterparty in _counterpartiesToSent)
					{
						if(!_canSend)
						{
							return;
						}

						var email = SelectPriorityEmail(counterparty.Emails);
						if(email == null)
						{
							withoutEmails += counterparty.FullName + "; ";
						}
						else
						{
							var storedEmail = new StoredEmail
							{
								State = StoredEmailStates.WaitingToSend,
								Author = _author,
								ManualSending = true,
								SendDate = DateTime.Now,
								StateChangeDate = DateTime.Now,
								Subject = MailSubject,
								RecipientAddress = email.Address,
								Guid = Guid.NewGuid()
							};

							unitOfWork.Save(storedEmail);

							var bulkEmail = new BulkEmail()
							{
								StoredEmail = storedEmail,
								Counterparty = counterparty
							};

							unitOfWork.Save(bulkEmail);

							try
							{
								unitOfWork.Commit();
								SendEmail(email.Address, counterparty.FullName, storedEmail);
							}
							catch(Exception e)
							{
								_logger.LogError(e, "Ошибка при отправке письма контрагенту {CounterpartyFullName}", counterparty?.FullName);
								sendingErrors += $"{ counterparty.FullName }; ";
							}
						}

						SendingProgressValue += 1;

						SendingProgressBarUpdated?.Invoke(this, EventArgs.Empty);
					}
				}

				if(withoutEmails.Length > 0)
				{
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"У следующих контрагентов отсутствует email:{ withoutEmails }");
				}

				if(sendingErrors.Length > 0)
				{
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, $"Возникла ошибка при отправке писем следующим контрагентам: { sendingErrors }");
				}

				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Завершено");

				Init();

				IsInSendingProcess = false;
			},
				() => CanExecute)
			);

		#endregion

		[PropertyChangedAlso(nameof(MailSubjectInfoDanger), nameof(CanExecute))]
		public string MailSubject
		{
			get => _mailSubject;
			set => SetField(ref _mailSubject, value);
		}

		public string MailSubjectInfo => $"{MailSubject?.Length ?? 0}/255 символов";

		public bool MailSubjectInfoDanger => MailSubject?.Length == 0 || MailSubject?.Length > 255;

		public string MailTextPart
		{
			get => _mailTextPart;
			set => SetField(ref _mailTextPart, value);
		}

		public string AttachmentsSizeInfo => $"{_attachmentsSize.ToString("F")} / 15 Мб";

		[PropertyChangedAlso(nameof(CanExecute))]
		public bool AttachmentsSizeInfoDanger => _attachmentsSize > 15;

		[PropertyChangedAlso(nameof(SendingDurationInfo), nameof(SendedCountInfo))]
		public double SendingProgressValue
		{
			get => _sendingProgressValue;
			set => SetField(ref _sendingProgressValue, value);
		}

		public double SendingProgressUpper => _counterpartiesToSent.Count;

		public string SendedCountInfo => $"Обработано {SendingProgressValue} из {SendingProgressUpper} контрагентов";

		public string SendingDurationInfo
		{
			get
			{
				TimeSpan time = TimeSpan.FromSeconds(SendingProgressUpper - SendingProgressValue);
				return time.ToString(@"hh\:mm\:ss");
			}
		}

		public bool IsInSendingProcess
		{
			get => _isInSendingProcess;
			private set => SetField(ref _isInSendingProcess, value);
		}

		public string RecepientInfo =>
			$"{_counterpartiesToSent.Count} / 1000 за раз ({_alreadySentCounterpartyIds.Count} " +
			$"уже получали письмо массовой рассылки в течение последних 2 часов)" +
			$"{Environment.NewLine}и {_alreadySentMonthCount} / 20000 в месяц писем вида массовой рассылки";

		[PropertyChangedAlso(nameof(CanExecute))]
		public bool RecepientInfoDanger =>
			(_counterpartiesToSent.Count - _alreadySentCounterpartyIds.Count) > 1000 || _alreadySentMonthCount > 20000;

		public bool CanExecute => !AttachmentsSizeInfoDanger && !MailSubjectInfoDanger && !RecepientInfoDanger;

		public int MonthsSinceUnsubscribing
		{
			get => _monthsSinceUnsubscribing;
			set
			{
				SetField(ref _monthsSinceUnsubscribing, value);
				Init();
			}
		}

		public bool IncludeOldUnsubscribed
		{
			get => _includeOldUnsubscribed;
			set
			{
				SetField(ref _includeOldUnsubscribed, value);
				Init();
			}
		}

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; }

		public EventHandler SendingProgressBarUpdated;

		public void Stop()
		{
			_canSend = false;
		}

		public void Dispose()
		{
			_rabbitMQChannel?.Dispose();
		}
	}
}

using EmailPrepareWorker.Prepares;
using EmailPrepareWorker.SendEmailMessageBuilders;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.Client;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using QS.Services;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Common;

namespace EmailPrepareWorker
{
	public class EmailPrepareWorker : TimerBackgroundServiceBase
	{
		private const string _queuesConfigurationSection = "Queues";
		private const string _emailSendExchangeParameter = "EmailSendExchange";
		private const string _emailSendKeyParameter = "EmailSendKey";

		private readonly string _emailSendKey;
		private readonly string _emailSendExchange;
		private readonly string _connectionString;
		private readonly ILogger<EmailPrepareWorker> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IModel _channel;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailSettings _emailSettings;
		private readonly IEmailDocumentPreparer _emailDocumentPreparer;
		private readonly IEmailSendMessagePreparer _emailSendMessagePreparer;
		private readonly int _instanceId;

		protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);

		public EmailPrepareWorker(
			IUserService userService,
			ILogger<EmailPrepareWorker> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IConfiguration configuration,
			MySqlConnector.MySqlConnectionStringBuilder mySqlConnectionStringBuilder,
			IModel channel,
			IEmailRepository emailRepository,
			IEmailSettings emailSettings,
			IEmailDocumentPreparer emailDocumentPreparer,
			IEmailSendMessagePreparer emailSendMessagePreparer)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_connectionString = mySqlConnectionStringBuilder.ConnectionString;

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_emailDocumentPreparer = emailDocumentPreparer ?? throw new ArgumentNullException(nameof(emailDocumentPreparer));
			_emailSendMessagePreparer = emailSendMessagePreparer ?? throw new ArgumentNullException(nameof(emailSendMessagePreparer));

			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

			var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version;

			_logger.LogInformation("Запущена сборка от {BuildDate}",
				new DateTime(2000, 1, 1)
					.AddDays(assemblyVersion.Build)
					.AddSeconds(assemblyVersion.Revision * 2));

			_emailSendKey = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendKeyParameter);
			_emailSendExchange = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendExchangeParameter);
			_channel.QueueDeclare(_emailSendKey, true, false, false, null);

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Email prepare worker"))
			{
				_instanceId = Convert.ToInt32(unitOfWork.Session
					.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
					.List<object>()
					.FirstOrDefault());
			}
		}

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				_logger.LogInformation("Email sending start at: {time}", DateTimeOffset.Now);

				await PrepareAndSendEmails();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
			}
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting Email Prepare Worker...");
			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping Email Prepare Worker...");
			return base.StopAsync(cancellationToken);
		}

		private async Task PrepareAndSendEmails()
		{
			SendEmailMessageBuilder emailSendMessageBuilder = null;

			using var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Document prepare worker");

			var emailsToSend = _emailRepository.GetEmailsForPreparingOrderDocuments(unitOfWork);

			foreach(var counterpartyEmail in emailsToSend)
			{
				try
				{
					_logger.LogInformation($"Found message to prepare for stored email: {counterpartyEmail.StoredEmail.Id}");

					if(counterpartyEmail.EmailableDocument == null)
					{
						counterpartyEmail.StoredEmail.State = StoredEmailStates.SendingError;
						counterpartyEmail.StoredEmail.Description = "Missing/deleted emailable document";
						unitOfWork.Save(counterpartyEmail.StoredEmail);
						unitOfWork.Commit();

						continue;
					}

					switch(counterpartyEmail.Type)
					{
						case CounterpartyEmailType.BillDocument:
						case CounterpartyEmailType.OrderWithoutShipmentForPayment:
						case CounterpartyEmailType.OrderWithoutShipmentForDebt:
						case CounterpartyEmailType.OrderWithoutShipmentForAdvancePayment:
							{
								emailSendMessageBuilder = new SendEmailMessageBuilder(unitOfWork, _emailSettings,
									_emailDocumentPreparer, counterpartyEmail, _instanceId);

								break;
							}
						case CounterpartyEmailType.UpdDocument:
							{
								emailSendMessageBuilder = new UpdSendEmailMessageBuilder(
									_emailSettings,
									unitOfWork, 
									_emailDocumentPreparer,
									counterpartyEmail,
									_instanceId);

								break;
							}
					}

					var properties = _channel.CreateBasicProperties();
					properties.Persistent = true;

					var message = _emailSendMessagePreparer.PrepareMessage(emailSendMessageBuilder, _connectionString);
					var serializedMessage = JsonSerializer.Serialize(message);
					var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

					_logger.LogInformation("Подготовлено {AttachmentsCount} вложений", message.Attachments.Count);

					_channel.BasicPublish(_emailSendExchange, _emailSendKey, properties, sendingBody);

					counterpartyEmail.StoredEmail.State = StoredEmailStates.WaitingToSend;
					unitOfWork.Save(counterpartyEmail.StoredEmail);
					unitOfWork.Commit();
				}
				catch(Exception ex)
				{
					_logger.LogError($"Failed to process counterparty email {counterpartyEmail.Id}: {ex.Message}");
				}
			}

			await Task.CompletedTask;
		}
	}
}

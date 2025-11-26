using EmailPrepareWorker.Prepares;
using EmailPrepareWorker.SendEmailMessageBuilders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Common;
using VodovozBusiness.Controllers;

namespace EmailPrepareWorker
{
	public class EmailPrepareWorker : TimerBackgroundServiceBase
	{
		private const string _queuesConfigurationSection = "Queues";
		private const string _emailSendExchangeParameter = "EmailSendExchange";
		private const string _emailSendKeyParameter = "EmailSendKey";

		private string _emailSendKey;
		private string _emailSendExchange;
		private int _instanceId;

		// Это для костыля с остановкой сервиса, при устранении утечек удалить вместе со связанным функционалом
		private int _crutchCounter = 0;
		private const int _crutchCounterLimit = 100;

		private bool _initialized = false;
		private readonly ILogger<EmailPrepareWorker> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IConfiguration _configuration;
		private readonly IModel _channel;
		private readonly IHostApplicationLifetime _hostApplicationLifetime;

		protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);

		public EmailPrepareWorker(
			IServiceScopeFactory serviceScopeFactory,
			ILogger<EmailPrepareWorker> logger,
			IConfiguration configuration,
			IModel channel,
			IHostApplicationLifetime hostApplicationLifetime)
		{
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_hostApplicationLifetime = hostApplicationLifetime;
			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

			var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version;

			_logger.LogInformation("Запущена сборка от {BuildDate}",
				new DateTime(2000, 1, 1)
					.AddDays(assemblyVersion.Build)
					.AddSeconds(assemblyVersion.Revision * 2));
		}

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				_logger.LogInformation("Email prepairing start at: {time}", DateTimeOffset.Now);

				await PrepareAndSendEmails();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
			}
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting Email Prepare Worker...");

			_emailSendKey = _configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendKeyParameter);
			_emailSendExchange = _configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendExchangeParameter);
			_channel.QueueDeclare(_emailSendKey, true, false, false, null);

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			using var settingsScope = _serviceScopeFactory.CreateScope();

			var unitOfWorkFactory = settingsScope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();

			using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Email prepare worker");

			_instanceId = Convert.ToInt32(unitOfWork.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			await base.StartAsync(cancellationToken);
			
			_logger.LogInformation(
				"Email Prepare worker started. Settings: EmailSendKey: {EmailSendKey}, EmailSendExchange: {EmailSendExchange}, InstanceId = {InstanceId}",
				_emailSendKey,
				_emailSendExchange,
				_instanceId);

			_initialized = true;
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping Email Prepare Worker...");
			return base.StopAsync(cancellationToken);
		}

		private async Task PrepareAndSendEmails()
		{
			if(!_initialized)
			{
				_logger.LogWarning("Not initialized, Prepairing aborted...");
				return;
			}

			using var prepareAndSendEmailsScope = _serviceScopeFactory.CreateScope();

			var unitOfWorkFactory = prepareAndSendEmailsScope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
			var emailRepository = prepareAndSendEmailsScope.ServiceProvider.GetRequiredService<IEmailRepository>();
			var emailSettings = prepareAndSendEmailsScope.ServiceProvider.GetRequiredService<IEmailSettings>();
			var emailDocumentPreparer = prepareAndSendEmailsScope.ServiceProvider.GetRequiredService<IEmailDocumentPreparer>();
			var emailSendMessagePreparer = prepareAndSendEmailsScope.ServiceProvider.GetRequiredService<IEmailSendMessagePreparer>();
			var mySqlConnectionStringBuilder = prepareAndSendEmailsScope.ServiceProvider.GetRequiredService<MySqlConnector.MySqlConnectionStringBuilder>();
			var edoAccountController = prepareAndSendEmailsScope.ServiceProvider.GetRequiredService<ICounterpartyEdoAccountController>();

			SendEmailMessageBuilder emailSendMessageBuilder = null;

			using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Document prepare worker");

			var emailsToSend = emailRepository.GetEmailsForPreparingOrderDocuments(unitOfWork);

			foreach(var counterpartyEmail in emailsToSend)
			{
				try
				{
					if(_crutchCounter > _crutchCounterLimit)
					{
						_logger.LogInformation("Email prepairing termination to prevent memory leak at: {time}", DateTimeOffset.Now);
						_hostApplicationLifetime.StopApplication();
						return;
					}

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
						case CounterpartyEmailType.EquipmentTransfer:
						case CounterpartyEmailType.OrderWithoutShipmentForPayment:
						case CounterpartyEmailType.OrderWithoutShipmentForDebt:
						case CounterpartyEmailType.OrderWithoutShipmentForAdvancePayment:
							{
								emailSendMessageBuilder = new SendEmailMessageBuilder(
									unitOfWork,
									emailSettings,
									emailRepository,
									emailDocumentPreparer,
									edoAccountController,
									counterpartyEmail,
									_instanceId);

								break;
							}
						case CounterpartyEmailType.UpdDocument:
							{
								emailSendMessageBuilder = new UpdSendEmailMessageBuilder(
									emailSettings,
									emailRepository,
									unitOfWork,
									emailDocumentPreparer,
									edoAccountController,
									counterpartyEmail,
									_instanceId);

								break;
							}
					}

					var properties = _channel.CreateBasicProperties();
					properties.Persistent = true;

					var message = emailSendMessagePreparer.PrepareMessage(emailSendMessageBuilder, mySqlConnectionStringBuilder.ConnectionString);
					var serializedMessage = JsonSerializer.Serialize(message);
					var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

					_logger.LogInformation("Подготовлено {AttachmentsCount} вложений", message.Attachments.Count);

					_channel.BasicPublish(_emailSendExchange, _emailSendKey, properties, sendingBody);

					counterpartyEmail.StoredEmail.State = StoredEmailStates.WaitingToSend;
					unitOfWork.Save(counterpartyEmail.StoredEmail);
					unitOfWork.Commit();

					_crutchCounter++;
					_logger.LogInformation("Email prepairing processed {ProcessedCount} of {ProcessMaxCount} (At maximum - service will be stopped)", _crutchCounter, _crutchCounterLimit);
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

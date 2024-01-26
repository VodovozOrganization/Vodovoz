using EmailPrepareWorker.Prepares;
using EmailPrepareWorker.SendEmailMessageBuilders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NHibernate.Driver.MySqlConnector;
using QS.DomainModel.UoW;
using QS.Project.DB;
using RabbitMQ.Client;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;

namespace EmailPrepareWorker
{
	public class EmailPrepareWorker : BackgroundService
	{
		private const string _queuesConfigurationSection = "Queues";
		private const string _emailSendExchangeParameter = "EmailSendExchange";
		private const string _emailSendKeyParameter = "EmailSendKey";

		private readonly string _emailSendKey;
		private readonly string _emailSendExchange;

		private readonly ILogger<EmailPrepareWorker> _logger;
		private readonly IModel _channel;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly IEmailDocumentPreparer _emailDocumentPreparer;
		private readonly IEmailSendMessagePreparer _emailSendMessagePreparer;
		private readonly TimeSpan _workDelay = TimeSpan.FromSeconds(5);
		private readonly int _instanceId;
		private readonly string _connectionString;

		public EmailPrepareWorker(
			ILogger<EmailPrepareWorker> logger,
			IConfiguration configuration,
			IModel channel,
			IEmailRepository emailRepository,
			IEmailParametersProvider emailParametersProvider,
			IEmailDocumentPreparer emailDocumentPreparer,
			IEmailSendMessagePreparer emailSendMessagePreparer)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_emailDocumentPreparer = emailDocumentPreparer ?? throw new ArgumentNullException(nameof(emailDocumentPreparer));
			_emailSendMessagePreparer = emailSendMessagePreparer ?? throw new ArgumentNullException(nameof(emailSendMessagePreparer));

			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

			_emailSendKey = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendKeyParameter);
			_emailSendExchange = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendExchangeParameter);
			_channel.QueueDeclare(_emailSendKey, true, false, false, null);

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var conStrBuilder = new MySqlConnectionStringBuilder();

			var databaseSection = configuration.GetSection("Database");

			conStrBuilder.Server = databaseSection.GetValue("Hostname", "localhost");
			conStrBuilder.Port = databaseSection.GetValue<uint>("Port", 3306);
			conStrBuilder.UserID = databaseSection.GetValue("Username", "");
			conStrBuilder.Password = databaseSection.GetValue("Password", "");
			conStrBuilder.Database = databaseSection.GetValue("DatabaseName", "");
			conStrBuilder.SslMode = MySqlSslMode.None;

			//QSMain.ConnectionString = conStrBuilder.GetConnectionString(true);
			_connectionString = conStrBuilder.GetConnectionString(true);

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
									 .Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
									 .ConnectionString(/*QSMain.ConnectionString*/_connectionString)
									 .Driver<MySqlConnectorDriver>();

			//OrmMain.ClassMappingList = new List<IOrmObjectMapping> (); // Нужно, чтобы запустился конструктор OrmMain

			OrmConfig.ConfigureOrm(db_config,
				new Assembly[] {
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
					Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
					Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase)),
					Assembly.GetAssembly(typeof(QS.Attachments.HibernateMapping.AttachmentMap)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
			});

			QS.HistoryLog.HistoryMain.Enable(conStrBuilder);

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Email prepare worker"))
			{
				_instanceId = Convert.ToInt32(unitOfWork.Session
					.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
					.List<object>()
					.FirstOrDefault());
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
			while(!stoppingToken.IsCancellationRequested)
			{
				await PrepareAndSendEmails();
				await Task.Delay(_workDelay, stoppingToken);
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

		private Task PrepareAndSendEmails()
		{
			SendEmailMessageBuilder emailSendMessageBuilder = null;
			
			try
			{
				using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Document prepare worker"))
				{
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
										emailSendMessageBuilder = new SendEmailMessageBuilder(_emailParametersProvider,
											_emailDocumentPreparer, counterpartyEmail, _instanceId);

										break;
								}
								case CounterpartyEmailType.UpdDocument: 
								{
									emailSendMessageBuilder = new UpdSendEmailMessageBuilder(_emailParametersProvider,
										_emailDocumentPreparer, counterpartyEmail, _instanceId);
										
									break;
								}
							}

							var sendingBody = _emailSendMessagePreparer.PrepareMessage(emailSendMessageBuilder, _connectionString);

							var properties = _channel.CreateBasicProperties();
							properties.Persistent = true;

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
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
			}

			return Task.CompletedTask;
		}
	}
}

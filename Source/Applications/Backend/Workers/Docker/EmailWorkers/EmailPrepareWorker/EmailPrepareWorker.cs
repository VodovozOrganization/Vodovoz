using fyiReporting.RDL;
using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Report;
using QSOrmProject;
using QSProjectsLib;
using RabbitMQ.Client;
using RabbitMQ.MailSending;
using RdlEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

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
		private readonly TimeSpan _workDelay = TimeSpan.FromSeconds(5);
		private readonly int _instanceId;

		public EmailPrepareWorker(ILogger<EmailPrepareWorker> logger, IConfiguration configuration, IModel channel, IEmailRepository emailRepository,
				IEmailParametersProvider emailParametersProvider)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));

			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

			_emailSendKey = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendKeyParameter);
			_emailSendExchange = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendExchangeParameter);
			_channel.QueueDeclare(_emailSendKey, true, false, false, null);

			var conStrBuilder = new MySqlConnectionStringBuilder();

			var databaseSection = configuration.GetSection("Database");

			conStrBuilder.Server = databaseSection.GetValue("Hostname", "localhost");
			conStrBuilder.Port = databaseSection.GetValue<uint>("Port", 3306);
			conStrBuilder.UserID = databaseSection.GetValue("Username", "");
			conStrBuilder.Password = databaseSection.GetValue("Password", "");
			conStrBuilder.Database = databaseSection.GetValue("DatabaseName", "");
			conStrBuilder.SslMode = MySqlSslMode.None;

			QSMain.ConnectionString = conStrBuilder.GetConnectionString(true);
			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
									 .Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
									 .ConnectionString(QSMain.ConnectionString);

			OrmMain.ClassMappingList = new List<IOrmObjectMapping> (); // Нужно, чтобы запустился конструктор OrmMain

			OrmConfig.ConfigureOrm(db_config,
				new Assembly[] {
					Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
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

		private async Task PrepareAndSendEmails()
		{
			try
			{
				var sendingMessage = new SendEmailMessage();

				using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Document prepare worker"))
				{
					var emailsToSend = _emailRepository.GetEmailsForPreparingOrderDocuments(unitOfWork);

					sendingMessage.From = new EmailContact
					{
						Name = _emailParametersProvider.DocumentEmailSenderName,
						Email = _emailParametersProvider.DocumentEmailSenderAddress
					};

					foreach(var counterpartyEmail in emailsToSend)
					{
						try
						{
							_logger.LogInformation($"Found message to prepare for stored email: { counterpartyEmail.StoredEmail.Id }");

							sendingMessage.To = new List<EmailContact>
							{
								new EmailContact
								{
									Name = counterpartyEmail.Counterparty.FullName,
									Email = counterpartyEmail.StoredEmail.RecipientAddress
								}
							};

							var document = counterpartyEmail.EmailableDocument;

							if(document == null)
							{
								counterpartyEmail.StoredEmail.State = StoredEmailStates.SendingError;
								counterpartyEmail.StoredEmail.Description = "Missing/deleted emailable document";
								unitOfWork.Save(counterpartyEmail.StoredEmail);
								unitOfWork.Commit();
								
								continue;
							}

							var template = document.GetEmailTemplate();

							sendingMessage.Subject = $"{ template.Title } { document.Title }";
							sendingMessage.TextPart = template.Text;
							sendingMessage.HTMLPart = template.TextHtml;

							var inlinedAttachments = new List<InlinedEmailAttachment>();

							foreach(var item in template.Attachments)
							{
								inlinedAttachments.Add(new InlinedEmailAttachment
								{
									ContentID = item.Key,
									ContentType = item.Value.MIMEType,
									Filename = item.Value.FileName,
									Base64Content = item.Value.Base64Content
								});
							}

							sendingMessage.InlinedAttachments = inlinedAttachments;

							var attachments = new List<EmailAttachment>
							{
								await PrepareDocument(document, counterpartyEmail.Type)
							};

							sendingMessage.Attachments = attachments;

							sendingMessage.Payload = new EmailPayload
							{
								Id = counterpartyEmail.StoredEmail.Id,
								Trackable = true,
								InstanceId = _instanceId
							};

							var serializedMessage = JsonSerializer.Serialize(sendingMessage);
							var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

							var properties = _channel.CreateBasicProperties();
							properties.Persistent = true;

							_channel.BasicPublish(_emailSendExchange, _emailSendKey, properties, sendingBody);

							counterpartyEmail.StoredEmail.State = StoredEmailStates.WaitingToSend;
							unitOfWork.Save(counterpartyEmail.StoredEmail);
							unitOfWork.Commit();
						}
						catch(Exception ex)
						{
							_logger.LogError($"Failed to process counterparty email { counterpartyEmail.Id }: { ex.Message }");
						}
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
			}
		}

		private static async Task<EmailAttachment> PrepareDocument(IEmailableDocument document, CounterpartyEmailType counterpartyEmailType)
		{
			bool wasHideSignature;
			ReportInfo ri;

			wasHideSignature = document.HideSignature;
			document.HideSignature = false;

			ri = document.GetReportInfo();

			document.HideSignature = wasHideSignature;

			using MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), QSMain.ConnectionString, OutputPresentationType.PDF, true);

			string documentDate = document.DocumentDate.HasValue ? "_" + document.DocumentDate.Value.ToString("ddMMyyyy") : "";

			string fileName = counterpartyEmailType.ToString();
			switch(counterpartyEmailType)
			{
				case CounterpartyEmailType.OrderDocument:
					fileName += $"_{ document.Order.Id }";
					break;
				default:
					fileName += $"_{ document.Id }";
					break;
			}

			fileName += $"_{ documentDate }.pdf";

			return await new ValueTask<EmailAttachment>(
				new EmailAttachment
				{
					Filename = fileName,
					ContentType = "application/pdf",
					Base64Content = Convert.ToBase64String(stream.GetBuffer())
				});
		}
	}
}

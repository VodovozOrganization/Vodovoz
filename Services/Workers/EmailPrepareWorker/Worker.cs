using fyiReporting.RDL;
using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Report;
using QSProjectsLib;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.MailSending;
using RdlEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;

namespace EmailPrepareWorker
{
	public class Worker : BackgroundService
	{
		private const string _queuesConfigurationSection = "Queues";
		private const string _emailPrepareQueueParameter = "EmailPrepareQueue";
		private const string _emailSendExchangeParameter = "EmailSendExchange";
		private const string _emailSendKeyParameter = "EmailSendKey";

		private readonly string _emailSendKey;
		private readonly string _emailSendExchange;
		private readonly string _emailPrepareQueue;

		private readonly ILogger<Worker> _logger;
		private readonly IModel _channel;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly AsyncEventingBasicConsumer _consumer;

		public Worker(ILogger<Worker> logger, IConfiguration configuration, IModel channel, IEmailRepository emailRepository,
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
			_emailSendKey = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendKeyParameter);
			_emailSendExchange = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendExchangeParameter);
			_emailPrepareQueue = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailPrepareQueueParameter);
			_channel.QueueDeclare(_emailPrepareQueue, true, false, false, null);
			_channel.QueueDeclare(_emailSendKey, true, false, false, null);
			_consumer = new AsyncEventingBasicConsumer(_channel);
			_consumer.Received += MessageRecieved;

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

			OrmConfig.ConfigureOrm(db_config,
				new System.Reflection.Assembly[] {
					System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.OrganizationMap)),
					System.Reflection.Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
					System.Reflection.Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
					System.Reflection.Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase)),
					System.Reflection.Assembly.GetAssembly(typeof(QS.Attachments.HibernateMapping.AttachmentMap))
			});

			QS.HistoryLog.HistoryMain.Enable();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_channel.BasicConsume(_emailPrepareQueue, false, _consumer);
			await Task.Delay(0, stoppingToken);
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

		private async Task MessageRecieved(object sender, BasicDeliverEventArgs e)
		{
			try
			{
				var body = e.Body;

				var message = JsonSerializer.Deserialize<PrepareEmailMessage>(body.Span);

				_logger.LogInformation($"Recieved message to prepare for stored Email: { message.StoredEmailId }");

				var sendingMessage = new SendEmailMessage();

				using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Document prepare worker"))
				{
					var storedEmail = _emailRepository.GetById(unitOfWork, message.StoredEmailId);

					sendingMessage.From = new EmailContact
					{
						Name = _emailParametersProvider.DocumentEmailSenderName,
						Email = _emailParametersProvider.DocumentEmailSenderAddress
					};

					sendingMessage.To = new List<EmailContact>
					{
						new EmailContact
						{
							Name = storedEmail.Order.Client.FullName,
							Email = storedEmail.RecipientAddress
						}
					};

					var documentType = storedEmail.DocumentType;
					var document = storedEmail.Order.OrderDocuments.FirstOrDefault(od => od.Type == documentType && od is IEmailableDocument) as IEmailableDocument;
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
						await PrepareDocument(document)
					};

					sendingMessage.Attachments = attachments;

					sendingMessage.Payload = new EmailPayload
					{
						Id = message.StoredEmailId,
						Trackable = true,
						InstanceId = Convert.ToInt32(unitOfWork.Session
							.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
							.List<object>()
							.FirstOrDefault())
					};
				}

				var serializedMessage = JsonSerializer.Serialize(sendingMessage);
				var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

				var properties = _channel.CreateBasicProperties();
				properties.Persistent = true;

				_channel.BasicPublish(_emailSendExchange, _emailSendKey, properties, sendingBody);

				_channel.BasicAck(e.DeliveryTag, false);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}
		}

		private static async Task<EmailAttachment> PrepareDocument(IEmailableDocument document)
		{
			bool wasHideSignature;
			ReportInfo ri;

			wasHideSignature = document.HideSignature;
			document.HideSignature = false;
			ri = document.GetReportInfo();
			document.HideSignature = wasHideSignature;

			using MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(), QSMain.ConnectionString, OutputPresentationType.PDF, true);

			string billDate = document.DocumentDate.HasValue ? "_" + document.DocumentDate.Value.ToString("ddMMyyyy") : "";

			return await new ValueTask<EmailAttachment>(
				new EmailAttachment
				{
					Filename = $"Bill_{ document.Order.Id }{ billDate }.pdf",
					ContentType = "application/pdf",
					Base64Content = Convert.ToBase64String(stream.GetBuffer())
				});
		}
	}
}

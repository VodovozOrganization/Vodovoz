using Mailjet.Api.Abstractions;
using Mailjet.Api.Abstractions.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.MailSending;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QSProjectsLib;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using MySql.Data.MySqlClient;

namespace EmailSendWorker
{
	public class EmailSendWorker : BackgroundService
	{
		private const string _mailjetConfigurationSection = "Mailjet";
		private const string _queuesConfigurationSection = "Queues";
		private const string _emailSendQueueParameter = "EmailSendQueue";
		private const string _sandboxModeParameter = "Sandbox";

		private const int _retriesCount = 5;
		private const int _retryDelay = 5 * 1000; // sec => milisec

		private readonly string _mailSendingQueueId;
		private readonly bool _sandboxMode;

		private readonly ILogger<EmailSendWorker> _logger;
		private readonly IModel _channel;
		private readonly SendEndpoint _sendEndpoint;
		private readonly AsyncEventingBasicConsumer _consumer;

		private readonly IEmailRepository _emailRepository;

		public EmailSendWorker(ILogger<EmailSendWorker> logger, IConfiguration configuration, IModel channel, SendEndpoint sendEndpoint, IEmailRepository emailRepository)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_channel = channel ?? throw new ArgumentNullException(nameof(channel));
			_sendEndpoint = sendEndpoint ?? throw new ArgumentNullException(nameof(sendEndpoint));
			_mailSendingQueueId = configuration.GetSection(_queuesConfigurationSection)
				.GetValue<string>(_emailSendQueueParameter);
			_channel.QueueDeclare(_mailSendingQueueId, true, false, false, null);
			_consumer = new AsyncEventingBasicConsumer(_channel);
			_consumer.Received += MessageRecieved;
			_sandboxMode = configuration.GetSection(_mailjetConfigurationSection).GetValue(_sandboxModeParameter, true);
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));

			try
			{
				var conStrBuilder = new MySqlConnectionStringBuilder();

				var databaseSection = configuration.GetSection("Database");

				conStrBuilder.Server = databaseSection.GetValue("Host", "localhost");
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
			catch(Exception ex)
			{
				_logger.LogCritical(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_channel.BasicConsume(_mailSendingQueueId, false, _consumer);
			await Task.Delay(0, stoppingToken);
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting Email Send Worker...");
			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping Email Send Worker...");
			return base.StopAsync(cancellationToken);
		}

		private async Task MessageRecieved(object sender, BasicDeliverEventArgs e)
		{
			ReadOnlyMemory<byte> body = null;

			try
			{
				body = e.Body;

				var message = JsonSerializer.Deserialize<SendEmailMessage>(body.Span);

				_logger.LogInformation($"Recieved message to send to recipients: { string.Join(", ", message.To.Select(recipient => recipient.Email)) }" +
					$" with subject: \"{ message.Subject }\", with { message.Attachments?.Count ?? 0 } attachments");

				var payload = new SendPayload
				{
					Messages = new[] { message },
					SandboxMode = _sandboxMode
				};

				for(var i = 0; i < _retriesCount; i++)
				{
					_logger.LogInformation($"Sending email { message.Payload.Id } { i }/{ _retriesCount + 1 }");

					try
					{
						var response = await _sendEndpoint.Send(payload);
						_logger.LogInformation("Response:\n" + JsonSerializer.Serialize(response));
						break;
					}
					catch(Exception exc)
					{
						_logger.LogError(exc, exc.Message);
						await Task.Delay(_retryDelay);

						if(i >= _retriesCount - 1)
						{
							FailedToSend(message);
						}

						continue;
					}
				}
			}
			finally
			{
				_logger.LogInformation("Free message from queue");
				_channel.BasicAck(e.DeliveryTag, false);
			}
		}

		private void FailedToSend(SendEmailMessage message)
		{
			_logger.LogInformation($"Failed to send email after { _retriesCount + 1 } attempts to send");

			try
			{
				using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Email send worker"))
				{
					var storedEmail = _emailRepository.GetById(unitOfWork, message.Payload.Id);

					if(storedEmail != null)
					{
						_logger.LogInformation($"Email {storedEmail.Id}: status {storedEmail.State}");

						storedEmail.State = StoredEmailStates.SendingError;

						unitOfWork.Save(storedEmail);
						unitOfWork.Commit();

						_logger.LogInformation($"Email {storedEmail.Id}: status changed to {storedEmail.State}");
					}
					else
					{
						_logger.LogWarning($"Stored Email with id: {message.Payload.Id} not found");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error during status change for stored email with id { message.Payload.Id } : { ex.Message }");
			}
		}
	}
}

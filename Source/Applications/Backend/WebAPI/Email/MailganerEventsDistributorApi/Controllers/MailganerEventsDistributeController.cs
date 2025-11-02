using Core.Infrastructure;
using MailganerEventsDistributorApi.DataAccess;
using MailganerEventsDistributorApi.DTO;
using Mailjet.Api.Abstractions.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Text;
using System.Text.Json;

namespace MailganerEventsDistributorApi.Controllers
{
	[Route("api")]
	[ApiController]
	public class MailganerEventsDistributeController : ControllerBase
	{
		private const string _queuesConfigurationSection = "Queues";
		private const string _messageBrockerConfigurationSection = "MessageBroker";
		private const string _emailStatusUpdateExchangeParameter = "EmailStatusUpdateExchange";
		private const string _emailStatusUpdateKeyParameter = "EmailStatusUpdateKey";

		private readonly ILogger<MailganerEventsDistributeController> _logger;
		private readonly IInstanceData _instanceData;
		private readonly RabbitMQConnectionFactory _queueConnectionFactory;
		private readonly IConfiguration _configuration;
		private readonly string _mailEventKey;
		private readonly string _mailEventExchange;

		public MailganerEventsDistributeController(ILogger<MailganerEventsDistributeController> logger, IInstanceData instanceData,
										  RabbitMQConnectionFactory queueConnectionFactory, IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_instanceData = instanceData ?? throw new ArgumentNullException(nameof(instanceData));
			_queueConnectionFactory = queueConnectionFactory ?? throw new ArgumentNullException(nameof(queueConnectionFactory));
			_mailEventExchange = configuration.GetSection(_queuesConfigurationSection).GetValue<string>(_emailStatusUpdateExchangeParameter);
			_mailEventKey = configuration.GetSection(_queuesConfigurationSection).GetValue<string>(_emailStatusUpdateKeyParameter);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/event")]
		public IActionResult EventCallback([FromBody] EmailEvent mailEvent)
		{
			foreach(var message in mailEvent.Messages)
			{
				_logger.LogInformation("Recieved event: {TrackId}", message.TrackId);

				if(message.TrackId.IsNullOrWhiteSpace())
				{
					continue;
				}

				NotifyEmailUpdated(message);
			}

			return Ok();
		}		

		[HttpGet]
		[AllowAnonymous]
		[Route("/Test")]
		public IActionResult Test()
		{
			_logger.LogInformation("Test");
			return Ok();
		}

		private void NotifyEmailUpdated(EmailEventMessage message)
		{
			var track = message.TrackId.Split('-');
			if(!int.TryParse(track[0], out var instanceId))
			{
				_logger.LogWarning("Не удалось получить InstanceId из трека письма: {TrackId}", message.TrackId);
				return;
			}
			if(!int.TryParse(track[1], out var trackId))
			{
				_logger.LogWarning("Не удалось получить InstanceId из трека письма: {TrackId}", message.TrackId);
				return;
			}

			var instance = _instanceData.GetInstanceByDatabaseId(instanceId);
			var messageBrockerSection = _configuration.GetSection(_messageBrockerConfigurationSection);

			var username = messageBrockerSection.GetValue<string>("Username");
			var password = messageBrockerSection.GetValue<string>("Password");

			var connection = _queueConnectionFactory.CreateConnection(instance.MessageBrockerHost, username, password, instance.MessageBrockerVirtualHost, instance.Port, messageBrockerSection.GetValue("UseSsl", true));
			var channel = connection.CreateModel();

			channel.QueueDeclare(_mailEventKey, true, false, false, null);

			var dateTimeRecieved = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).DateTime.ToLocalTime();

			var payload = new EmailPayload
			{
				Trackable = true,
				Id = trackId,
				InstanceId = instanceId
			};

			var eventMessage = new UpdateStoredEmailStatusMessage
			{
				EventPayload = payload,
				RecievedAt = dateTimeRecieved,
				Status = ConvertStatus(message.Status),
				MailjetMessageId = message.MessageId,
				ErrorInfo = message.Reason
			};

			var serializedMessage = JsonSerializer.Serialize(eventMessage);
			var body = Encoding.UTF8.GetBytes(serializedMessage);

			var properties = channel.CreateBasicProperties();
			properties.Persistent = true;

			channel.BasicPublish(_mailEventExchange, _mailEventKey, false, properties, body);

			return;
		}

		private MailEventType ConvertStatus(string status)
		{
			switch(status)
			{
				case "accepted":
				case "delivered":
					return MailEventType.sent;
				case "duplicate":
				case "failed":
					return MailEventType.bounce;
				case "open":
					return MailEventType.open;
				case "click":
					return MailEventType.click;
				case "fbl":
					return MailEventType.spam;
				case "unsubscribe":
					return MailEventType.unsub;
				default:
					return MailEventType.bounce;
			}
		}
	}
}

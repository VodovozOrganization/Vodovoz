using Mailjet.Api.Abstractions.Events;
using MailjetEventsDistributorAPI.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Text;
using System.Text.Json;

namespace MailjetEventsDistributorAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MailjetEventsDistributeController : ControllerBase
	{
		private const string _queuesConfigurationSection = "Queues";
		private const string _messageBrockerConfigurationSection = "MessageBroker";
		private const string _emailStatusUpdateExchangeParameter = "EmailStatusUpdateExchange";
		private const string _emailStatusUpdateKeyParameter = "EmailStatusUpdateKey";

		private readonly ILogger<MailjetEventsDistributeController> _logger;
		private readonly IInstanceData _instanceData;
		private readonly RabbitMQConnectionFactory _queueConnectionFactory;
		private readonly IConfiguration _configuration;
		private readonly string _mailEventKey;
		private readonly string _mailEventExchange;

		public MailjetEventsDistributeController(ILogger<MailjetEventsDistributeController> logger, IInstanceData instanceData,
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
		[Route("/EventCallback")]
		public IActionResult EventCallback([FromBody] MailEvent mailEvent)
		{
			_logger.LogInformation($"Recieved Event { mailEvent.EventType }");

			if(mailEvent.EventType == MailEventType.sent)
			{
				return SentEventCallback(mailEvent);
			}

			if(mailEvent.EventType == MailEventType.open)
			{
				return OpenEventCallback(mailEvent);
			}

			if(mailEvent.EventType == MailEventType.click)
			{
				return ClickEventCallback(mailEvent);
			}

			if(mailEvent.EventType == MailEventType.bounce)
			{
				return BounceEventCallback(mailEvent);
			}

			if(mailEvent.EventType == MailEventType.blocked)
			{
				return BlockedEventCallback(mailEvent);
			}

			if(mailEvent.EventType == MailEventType.spam)
			{
				return SpamEventCallback(mailEvent);
			}

			if(mailEvent.EventType == MailEventType.unsub)
			{
				return UnsubscribeEventCallback(mailEvent);
			}

			return BadRequest("Something goes wrong 0_o");
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/SentEventCallback")]
		public IActionResult SentEventCallback([FromBody] MailEvent mailSentEvent)
		{
			_logger.LogInformation($"Recieved Sent Event for: { mailSentEvent.MessageGuid }");
			return SendMessageToBrocker(mailSentEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/OpenEventCallback")]
		public IActionResult OpenEventCallback([FromBody] MailEvent mailOpenEvent)
		{
			_logger.LogInformation($"Recieved Open Event for: { mailOpenEvent.MessageGuid }");
			return SendMessageToBrocker(mailOpenEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/ClickEventCallback")]
		public IActionResult ClickEventCallback([FromBody] MailEvent mailClickEvent)
		{
			_logger.LogInformation($"Recieved Click Event for: { mailClickEvent.MessageGuid }");
			return SendMessageToBrocker(mailClickEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/BounceEventCallback")]
		public IActionResult BounceEventCallback([FromBody] MailEvent mailBounceEvent)
		{
			_logger.LogInformation($"Recieved Bounce Event for: { mailBounceEvent.MessageGuid }");
			return SendMessageToBrocker(mailBounceEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/BlockedEventCallback")]
		public IActionResult BlockedEventCallback([FromBody] MailEvent mailBlockedEvent)
		{
			_logger.LogInformation($"Recieved Blocked Event for: { mailBlockedEvent.MessageGuid }");
			return SendMessageToBrocker(mailBlockedEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/SpamEventCallback")]
		public IActionResult SpamEventCallback([FromBody] MailEvent mailSpamEvent)
		{
			_logger.LogInformation($"Recieved Spam Event for: { mailSpamEvent.MessageGuid }");
			return SendMessageToBrocker(mailSpamEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/UnsubscribeEventCallback")]
		public IActionResult UnsubscribeEventCallback([FromBody] MailEvent mailUnsubscribeEvent)
		{
			_logger.LogInformation($"Recieved Unsubscribe Event for: { mailUnsubscribeEvent.MessageGuid }");
			return SendMessageToBrocker(mailUnsubscribeEvent);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("/Test")]
		public IActionResult Test()
		{
			return new OkResult();
		}

		private IActionResult SendMessageToBrocker<TMailjetEvent>(TMailjetEvent mailjetEvent) where TMailjetEvent : MailEvent
		{
			EmailPayload payload = null;

			if(!string.IsNullOrWhiteSpace(mailjetEvent.Payload))
			{
				var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				payload = JsonSerializer.Deserialize<EmailPayload>(mailjetEvent.Payload, jsonOptions);
			}

			if(payload is { Trackable: true })
			{
				var instance = _instanceData.GetInstanceByDatabaseId(payload.InstanceId);
				var messageBrockerSection = _configuration.GetSection(_messageBrockerConfigurationSection);

				var username = messageBrockerSection.GetValue<string>("Username");
				var password = messageBrockerSection.GetValue<string>("Password");

				var connection = _queueConnectionFactory.CreateConnection(instance.MessageBrockerHost, username, password, instance.MessageBrockerVirtualHost, instance.Port, messageBrockerSection.GetValue("UseSsl", true));
				var channel = connection.CreateModel();

				channel.QueueDeclare(_mailEventKey, true, false, false, null);

				var dateTimeRecieved = DateTimeOffset.FromUnixTimeSeconds(mailjetEvent.Time).DateTime.ToLocalTime();

				var eventMessage = new UpdateStoredEmailStatusMessage
				{
					EventPayload = payload,
					RecievedAt = dateTimeRecieved,
					Status = mailjetEvent.EventType,
					MailjetMessageId = mailjetEvent.MessageId.ToString(),
					ErrorInfo = ""
				};

				if(mailjetEvent is MailBlockedEvent mble)
				{
					eventMessage.ErrorInfo = $"Error: { mble.Error }";
				}

				if(mailjetEvent is MailBounceEvent mbe)
				{
					eventMessage.ErrorInfo = $"Error: { mbe.Error }, Description: { mbe.Comment }";
				}

				var serializedMessage = JsonSerializer.Serialize(eventMessage);
				var body = Encoding.UTF8.GetBytes(serializedMessage);

				var properties = channel.CreateBasicProperties();
				properties.Persistent = true;

				channel.BasicPublish(_mailEventExchange, _mailEventKey, false, properties, body);
			}

			return new OkResult();
		}
	}
}

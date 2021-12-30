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
using System.Threading.Tasks;

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
		public async Task<IActionResult> EventCallback([FromBody] MailEvent mailSentEvent)
		{
			_logger.LogInformation($"Recieved Event { mailSentEvent.EventType }");

			if(mailSentEvent.EventType == MailEventType.sent)
			{
				return await new ValueTask<IActionResult>(RedirectToActionPreserveMethod(nameof(SentEventCallback)));
			}

			if(mailSentEvent.EventType == MailEventType.open)
			{
				return await new ValueTask<IActionResult>(RedirectToActionPreserveMethod(nameof(OpenEventCallback)));
			}

			if(mailSentEvent.EventType == MailEventType.click)
			{
				return await new ValueTask<IActionResult>(RedirectToActionPreserveMethod(nameof(ClickEventCallback)));
			}

			if(mailSentEvent.EventType == MailEventType.bounce)
			{
				return await new ValueTask<IActionResult>(RedirectToActionPreserveMethod(nameof(BounceEventCallback)));
			}

			if(mailSentEvent.EventType == MailEventType.blocked)
			{
				return await new ValueTask<IActionResult>(RedirectToActionPreserveMethod(nameof(BlockedEventCallback)));
			}

			if(mailSentEvent.EventType == MailEventType.spam)
			{
				return await new ValueTask<IActionResult>(RedirectToActionPreserveMethod(nameof(SpamEventCallback)));
			}

			if(mailSentEvent.EventType == MailEventType.unsub)
			{
				return await new ValueTask<IActionResult>(RedirectToActionPreserveMethod(nameof(UnsubscribeEventCallback)));
			}

			return BadRequest("Something goes wrong 0_o");
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/SentEventCallback")]
		public async Task SentEventCallback([FromBody] MailSentEvent mailSentEvent)
		{
			_logger.LogInformation($"Recieved Sent Event for: { mailSentEvent.MessageGuid }");
			await SendMessageToBrocker(mailSentEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/OpenEventCallback")]
		public async Task OpenEventCallback([FromBody] MailOpenEvent mailOpenEvent)
		{
			_logger.LogInformation($"Recieved Open Event for: { mailOpenEvent.MessageGuid }");
			await SendMessageToBrocker(mailOpenEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/ClickEventCallback")]
		public async Task ClickEventCallback([FromBody] MailClickEvent mailClickEvent)
		{
			_logger.LogInformation($"Recieved Click Event for: { mailClickEvent.MessageGuid }");
			await SendMessageToBrocker(mailClickEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/BounceEventCallback")]
		public async Task BounceEventCallback([FromBody] MailBounceEvent mailBounceEvent)
		{
			_logger.LogInformation($"Recieved Bounce Event for: { mailBounceEvent.MessageGuid }");
			await SendMessageToBrocker(mailBounceEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/BlockedEventCallback")]
		public async Task BlockedEventCallback([FromBody] MailBlockedEvent mailBlockedEvent)
		{
			_logger.LogInformation($"Recieved Blocked Event for: { mailBlockedEvent.MessageGuid }");
			await SendMessageToBrocker(mailBlockedEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/SpamEventCallback")]
		public async Task SpamEventCallback([FromBody] MailSpamEvent mailSpamEvent)
		{
			_logger.LogInformation($"Recieved Spam Event for: { mailSpamEvent.MessageGuid }");
			await SendMessageToBrocker(mailSpamEvent);
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/UnsubscribeEventCallback")]
		public async Task UnsubscribeEventCallback([FromBody] MailUnsubscribeEvent mailUnsubscribeEvent)
		{
			_logger.LogInformation($"Recieved Unsubscribe Event for: { mailUnsubscribeEvent.MessageGuid }");
			await SendMessageToBrocker(mailUnsubscribeEvent);
		}

		private async Task SendMessageToBrocker<TMailjetEvent>(TMailjetEvent mailjetEvent) where TMailjetEvent : MailEvent
		{
			var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

			var payload = JsonSerializer.Deserialize<EmailPayload>(mailjetEvent.Payload, jsonOptions);

			if(payload.Trackable)
			{
				var instance = _instanceData.GetInstanceByDatabaseId(payload.InstanceId);
				var messageBrockerSection = _configuration.GetSection(_messageBrockerConfigurationSection);

				var username = messageBrockerSection.GetValue<string>("Username");
				var password = messageBrockerSection.GetValue<string>("Password");

				var connection = _queueConnectionFactory.CreateConnection(instance.MessageBrockerHost, username, password, instance.MessageBrockerVirtualHost);
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
		}
	}
}

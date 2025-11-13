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
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MailganerEventsDistributorApi.Controllers
{
	[Route("api")]
	[ApiController]
	public class MailganerEventsDistributeController : ControllerBase
	{
		private readonly ILogger<MailganerEventsDistributeController> _logger;
		private readonly IInstanceData _instanceData;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public MailganerEventsDistributeController(
			ILogger<MailganerEventsDistributeController> logger,
			IInstanceData instanceData,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_instanceData = instanceData ?? throw new ArgumentNullException(nameof(instanceData));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
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

			PublishMessage(message, instanceId, trackId);
		}

		private void PublishMessage(EmailEventMessage message, int instanceId, int trackId)
		{
			try
			{
				var instance = _instanceData.GetInstanceByDatabaseId(instanceId);
				
				IPublishEndpoint endpoint = null;
				using var scope = _serviceScopeFactory.CreateScope();

				switch (instance.MessageBrockerVirtualHost)
				{
					case "email_host":
						endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
						break;
					case "email_host_test":
						endpoint = scope.ServiceProvider.GetRequiredService<Bind<IEmailDevBus, IPublishEndpoint>>().Value;
						break;
					default:
						_logger.LogError("Неизвестный хост {VirtualHost}", instance.MessageBrockerVirtualHost);
						return;
				}

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
				
				endpoint.Publish(eventMessage);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при попытке отправки уведомления о смене статуса отправки письма");
			}
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

using Core.Infrastructure;
using MailganerEventsDistributorApi.DataAccess;
using MailganerEventsDistributorApi.DTO;
using Mailjet.Api.Abstractions.Events;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.Infrastructure;
using RabbitMQ.MailSending;
using System;
using System.Threading.Tasks;

namespace MailganerEventsDistributorApi.Controllers
{
	[Route("api")]
	[ApiController]
	public class MailganerEventsDistributeController : ControllerBase
	{
		private readonly ILogger<MailganerEventsDistributeController> _logger;
		private readonly IInstanceData _instanceData;
		private readonly IPublishEndpoint _publishEndpoint;
		// Вторая шина приложения (IEmailDevBus, тестовый vhost RabbitMQ email_host_test) для событий тестовых баз.
		// Публикует напрямую, без outbox: таблицы outbox тестового vhost'а никто не отправляет. При переходе на один vhost пересмотреть
		private readonly IPublishEndpoint _devPublishEndpoint;
		private readonly IUnitOfWork _unitOfWork;

		/// <param name="publishEndpoint">Основная шина (vhost email_host): через outbox, сообщения сохраняются при коммите UoW запроса</param>
		/// <param name="devPublishEndpoint">Вторая шина (тестовый vhost email_host_test): напрямую, без outbox</param>
		/// <param name="unitOfWork">UoW запроса: в его транзакцию пишутся сообщения основной шины</param>
		public MailganerEventsDistributeController(
			ILogger<MailganerEventsDistributeController> logger,
			IInstanceData instanceData,
			IPublishEndpoint publishEndpoint,
			Bind<IEmailDevBus, IPublishEndpoint> devPublishEndpoint,
			IUnitOfWork unitOfWork)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_instanceData = instanceData ?? throw new ArgumentNullException(nameof(instanceData));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
			_devPublishEndpoint = (devPublishEndpoint ?? throw new ArgumentNullException(nameof(devPublishEndpoint))).Value;
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("/event")]
		public async Task<IActionResult> EventCallback([FromBody] EmailEvent mailEvent)
		{
			foreach(var message in mailEvent.Messages)
			{
				_logger.LogInformation("Recieved event: {TrackId}", message.TrackId);

				if(message.TrackId.IsNullOrWhiteSpace())
				{
					continue;
				}

				await NotifyEmailUpdated(message);
			}

			// События пачки сохраняются в outbox одной транзакцией и уходят после коммита.
			// Если сохранить не удалось, Mailganer получит ошибку и повторит запрос
			await _unitOfWork.CommitAsync();

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

		private async Task NotifyEmailUpdated(EmailEventMessage message)
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

			await PublishMessage(message, instanceId, trackId);
		}

		private async Task PublishMessage(EmailEventMessage message, int instanceId, int trackId)
		{
			try
			{
				var instance = _instanceData.GetInstanceByDatabaseId(instanceId);

				IPublishEndpoint endpoint;

				switch(instance.MessageBrockerVirtualHost)
				{
					case "email_host":
						endpoint = _publishEndpoint;
						break;
					case "email_host_test":
						endpoint = _devPublishEndpoint;
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

				await endpoint.Publish(eventMessage);
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

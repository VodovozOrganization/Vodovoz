using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Errors;

namespace Vodovoz.FirebaseCloudMessaging
{
	public class FirebaseCloudMessagingService : IFirebaseCloudMessagingService
	{
		private readonly ILogger<FirebaseCloudMessagingService> _logger;
		private readonly FirebaseMessaging _firebaseMessaging;

		public FirebaseCloudMessagingService(
			ILogger<FirebaseCloudMessagingService> logger,
			FirebaseMessaging firebaseMessaging)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_firebaseMessaging = firebaseMessaging
				?? throw new ArgumentNullException(nameof(firebaseMessaging));
		}

		public async Task<Result> SendFastDeliveryAddressCanceledMessage(string recipientToken, int orderId)
		{
			return await SendMessage(
				recipientToken,
				"Отмена заказа с доставкой за час",
				$"Заказ №{orderId} с доставкой за час отменен");
		}

		public async Task<Result> SendMessage(string recipientToken, string title, string body)
		{
			try
			{
				var message = new Message
				{
					Token = recipientToken,
					Notification = new Notification
					{
						Title = title,
						Body = body
					}
				};

				var messageId = await _firebaseMessaging.SendAsync(message);

				if(!string.IsNullOrWhiteSpace(messageId))
				{
					_logger.LogInformation("Сообщение отправлено успешно: {FirebaseCloudMessageId}", messageId);
				}

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при отправки PUSH-сообщения: {ExceptionMessage}", ex.Message);
				return Result.Failure(FirebaseCloudMessagingServiceErrors.SendingError);
			}
		}

		public async Task<Result> SendWakeUpMessage(string recipientToken)
		{
			try
			{
				var message = new Message
				{
					Token = recipientToken,
					Android = new AndroidConfig
					{
						Priority = Priority.High,
					},
					Apns = new ApnsConfig
					{
						Aps = new Aps
						{
							ContentAvailable = true,
						}
					},
				};

				var messageId = await _firebaseMessaging.SendAsync(message);

				if(!string.IsNullOrWhiteSpace(messageId))
				{
					_logger.LogInformation("Сообщение отправлено успешно: {FirebaseCloudMessageId}", messageId);
				}

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при отправки PUSH-сообщения: {ExceptionMessage}", ex.Message);
				return Result.Failure(FirebaseCloudMessagingServiceErrors.SendingError);
			}
		}
	}
}

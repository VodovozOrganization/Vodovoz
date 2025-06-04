using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Options;

namespace Vodovoz.FirebaseCloudMessaging
{
	public class FirebaseCloudMessagingService : IFirebaseCloudMessagingService
	{
		private readonly ILogger<FirebaseCloudMessagingService> _logger;
		private readonly IOptionsMonitor<PushNotificationSettings> _optionsMonitor;
		private readonly FirebaseMessaging _firebaseMessaging;

		public FirebaseCloudMessagingService(
			ILogger<FirebaseCloudMessagingService> logger,
			IOptionsMonitor<PushNotificationSettings> optionsMonitor,
			FirebaseMessaging firebaseMessaging)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_optionsMonitor = optionsMonitor;
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

		public async Task<Result> SendFastDeliveryAddressTransferedMessage(string recipientToken, int orderId)
		{
			return await SendMessage(
				recipientToken,
				"Добавление заказа с доставкой за час",
				$"Заказ №{orderId} с доставкой за час был добавлен в ваш маршрутный лист");
		}

		public async Task<Result> SendMessage(string recipientToken, string title, string body, Dictionary<string, string> data = null)
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

				if(data != null)
				{
					message.Data = data;
				}

				var options = _optionsMonitor.CurrentValue;

				string messageId = string.Empty;

				if(options.PushNotificationsEnabled)
				{
					messageId = await _firebaseMessaging.SendAsync(message);
				}
				else
				{
					_logger.LogDebug("Передача PUSH-сообщений отключена, тело запроса: {@Message}", message);
				}

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

				var options = _optionsMonitor.CurrentValue;

				string messageId = string.Empty;

				if(options.WakeUpNotificationsEnabled)
				{
					messageId = await _firebaseMessaging.SendAsync(message);
				}
				else
				{
					_logger.LogDebug("Передача WakeUp PUSH-сообщений отключена, тело запроса: {@Message}", message);
				}

				if(!string.IsNullOrWhiteSpace(messageId))
				{
					_logger.LogInformation("Сообщение отправлено успешно: {FirebaseCloudMessageId}", messageId);
				}

				return Result.Success();
			}
			catch(FirebaseMessagingException firebaseMessagingException)
				when(firebaseMessagingException.MessagingErrorCode == MessagingErrorCode.Unregistered)
			{
				_logger.LogError(firebaseMessagingException, "Ошибка отправки PUSH-сообщения, токен {Token} не зарегистрирован", recipientToken);
				return Result.Failure(FirebaseCloudMessagingServiceErrors.Unregistered);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошло исключение при отправки PUSH-сообщения: {ExceptionMessage}", ex.Message);
				return Result.Failure(FirebaseCloudMessagingServiceErrors.SendingError);
			}
		}
	}
}

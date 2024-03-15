using FirebaseCloudMessaging.Client.Options;
using Google.Apis.FirebaseCloudMessaging.v1.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Errors;
using GoogleFirebaseCloudMessagingService = Google.Apis.FirebaseCloudMessaging.v1.FirebaseCloudMessagingService;

namespace Vodovoz.FirebaseCloudMessaging
{
	public class FirebaseCloudMessagingService : IFirebaseCloudMessagingService
	{
		private readonly ILogger<FirebaseCloudMessagingService> _logger;
		private readonly IOptions<FirebaseCloudMessagingSettings> _options;
		private readonly GoogleFirebaseCloudMessagingService _cloudMessagingService;

		public FirebaseCloudMessagingService(
			ILogger<FirebaseCloudMessagingService> logger,
			IOptions<FirebaseCloudMessagingSettings> options,
			GoogleFirebaseCloudMessagingService cloudMessagingService)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_options = options
				?? throw new ArgumentNullException(nameof(options));
			_cloudMessagingService = cloudMessagingService
				?? throw new ArgumentNullException(nameof(cloudMessagingService));
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
				var result = await _cloudMessagingService.Projects.Messages.Send(new SendMessageRequest
				{
					Message = new Message
					{
						Token = recipientToken,
						Notification = new Notification
						{
							Title = title,
							Body = body
						}
					}
				}, _options.Value.ProjectId).ExecuteAsync();


				if(!string.IsNullOrWhiteSpace(result.Name))
				{
					_logger.LogInformation("Сообщение отправлено успешно: {FirebaseCloudMessageName}", result.Name);
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
				var result = await _cloudMessagingService.Projects.Messages.Send(new SendMessageRequest
				{
					Message = new Message
					{
						Token = recipientToken,
						Android = new AndroidConfig
						{
							Priority = "high",
						},
						Apns = new ApnsConfig
						{
							Headers = new Dictionary<string, string>
							{
								{ "apns-priority" , "10" }
							}
						}
					}
				}, _options.Value.ProjectId).ExecuteAsync();


				if(!string.IsNullOrWhiteSpace(result.Name))
				{
					_logger.LogInformation("Сообщение отправлено успешно: {FirebaseCloudMessageName}", result.Name);
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

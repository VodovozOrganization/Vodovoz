using Firebase.Client.Exceptions;
using FirebaseCloudMessaging.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Application.FirebaseCloudMessaging;
using Vodovoz.Errors;

namespace Vodovoz.FirebaseCloudMessaging
{
	public class FirebaseCloudMessagingService : IFirabaseCloudMessagingService
	{
		private readonly ILogger<FirebaseCloudMessagingService> _logger;
		private readonly IFirebaseCloudMessagingClientService _firebaseCloudMessagingClientService;

		public FirebaseCloudMessagingService(
			ILogger<FirebaseCloudMessagingService> logger,
			IFirebaseCloudMessagingClientService firebaseCloudMessagingClientService)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_firebaseCloudMessagingClientService = firebaseCloudMessagingClientService
				?? throw new ArgumentNullException(nameof(firebaseCloudMessagingClientService));
		}

		public async Task<Result> SendMessage(string recipientToken, string title, string body, object data = null)
		{
			try
			{
				await _firebaseCloudMessagingClientService.SendMessage(recipientToken, title, body, data);
				return Result.Success();
			}
			catch(FirebaseCloudMessagingClientServiceException ex)
			{
				_logger.LogError(ex, "Произошло исключение при отправки PUSH-сообщения: {ExceptionMessage}", ex.Message);
				return Result.Failure(FirebaseCloudMessagingServiceErrors.SendingError);
			}
		}

		public async Task<Result> SendFastDeliveryAddressCanceledMessage(string recipientToken, int orderId)
		{
			return await SendMessage(
				recipientToken,
				"Отмена заказа с доставкой за час",
				$"Заказ №{orderId} с доставкой за час отменен");
		}

		public async Task<Result> SendWakeUpMessage(string recipientToken)
		{
			return await SendMessage(recipientToken, "Веселый водовоз", string.Empty, new
			{
				priority = "high",
				content_available = true
			});
		}
	}
}

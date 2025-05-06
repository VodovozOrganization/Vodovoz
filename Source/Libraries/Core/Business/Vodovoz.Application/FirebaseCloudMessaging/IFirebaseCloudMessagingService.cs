﻿using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.FirebaseCloudMessaging
{
	public interface IFirebaseCloudMessagingService
	{
		Task<Result> SendFastDeliveryAddressCanceledMessage(string recipientToken, int orderId);
		Task<Result> SendFastDeliveryAddressTransferedMessage(string recipientToken, int orderId);
		Task<Result> SendMessage(string recipientToken, string title, string body);
		Task<Result> SendWakeUpMessage(string recipientToken);
	}
}

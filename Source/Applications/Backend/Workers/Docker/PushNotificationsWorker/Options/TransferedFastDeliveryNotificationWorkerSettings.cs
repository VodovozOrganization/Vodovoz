using System;

namespace PushNotificationsWorker.Options
{
	public sealed class TransferedFastDeliveryNotificationWorkerSettings
	{
		public int IntervalSeconds { get; set; }

		public TimeSpan Interval => TimeSpan.FromSeconds(IntervalSeconds);
	}
}

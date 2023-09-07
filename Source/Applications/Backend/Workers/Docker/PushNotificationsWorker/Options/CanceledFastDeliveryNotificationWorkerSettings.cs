using System;

namespace PushNotificationsWorker.Options
{
	public sealed class CanceledFastDeliveryNotificationWorkerSettings
	{
		public int IntervalSeconds { get; set; }

		public TimeSpan Interval => TimeSpan.FromSeconds(IntervalSeconds);
	}
}

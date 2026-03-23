namespace CustomerNotifications.Contracts
{
	/// <summary>
	/// Константы, используемые в контракте уведомлений.
	/// </summary>
	public static class CustomerNotificationsConstants
	{
		/// <summary>
		/// Имя очереди для публикации и получения уведомлений.
		/// </summary>
		public static string QueueName => "customer-notifications";
	}
}

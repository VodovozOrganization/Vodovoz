namespace TransactionalOutbox.MassTransit
{
	/// <summary>
	/// Логические имена шин (vhost'ов), сообщения которых хранятся в outbox.
	/// Служба отправки каждой шины забирает только свои сообщения.
	/// После перехода на один vhost RabbitMQ шина останется одна: здесь останется одно имя и одна пара таблиц outbox,
	/// а классы записей и маппинги остальных шин (Domain/Buses, Mapping/Buses) и выбор шины в OutboxRecordFactory удаляются.
	/// </summary>
	public static class OutboxBusNames
	{
		/// <summary>
		/// vhost телефонии и ЭДО (настройки Pacs.*)
		/// </summary>
		public const string Pacs = "pacs";

		/// <summary>
		/// vhost уведомлений
		/// </summary>
		public const string Notifications = "notifications";

		/// <summary>
		/// vhost почты (email_host)
		/// </summary>
		public const string Email = "email";

		/// <summary>
		/// vhost документов Такском (taxcom_edo_host)
		/// </summary>
		public const string Taxcom = "taxcom";

		/// <summary>
		/// vhost онлайн-заказов (online_orders)
		/// </summary>
		public const string OnlineOrders = "online_orders";
	}
}

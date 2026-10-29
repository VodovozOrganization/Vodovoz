using TransactionalOutbox.MassTransit;

namespace TransactionalOutbox.EntityFrameworkCore
{
	/// <summary>
	/// Шина, сообщения которой отправляет этот сервис: по ней <see cref="OutboxDbContext"/> выбирает таблицы.
	/// Регистрируется в <see cref="OutboxBusRegistrationExtensions.AddVodovozOutbox"/>.
	/// После перехода на один vhost RabbitMQ не нужен: шина одна
	/// </summary>
	public sealed class OutboxDeliveryBusName
	{
		public OutboxDeliveryBusName(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Имя шины, см. <see cref="OutboxBusNames"/>
		/// </summary>
		public string Name { get; }
	}
}

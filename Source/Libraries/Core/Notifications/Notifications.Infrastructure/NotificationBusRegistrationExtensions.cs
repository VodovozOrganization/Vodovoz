using MassTransit;
using MassTransit.Configuration;
using MassTransit.Transports;
using Notifications.Infrastructure.Mapping;
using QS.Project.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using TransactionalOutbox.MassTransit;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Регистрация шины уведомлений «только для записи» (<see cref="INotificationsBus"/>).
	/// Временное решение, пока уведомления живут в отдельном vhost'е notifications: десктопу и сервисам с другим
	/// основным vhost'ом нужно писать уведомления в таблицы notifications_*, но не подключаться к этому vhost'у.
	/// Для этого здесь два обхода MassTransit, которых нет в его документации:
	/// <list type="bullet">
	/// <item>адрес-заглушка <see cref="_outboxOnlyHost"/>: MassTransit формирует сообщение только по настроенному транспорту
	/// с хостом, а служба отправки берёт из адреса назначения имя exchange, хост не использует (MassTransit 8.0.12, проверено на стенде);</item>
	/// <item>шина собирается внутренним классом MassTransit <c>ServiceCollectionBusConfigurator</c>, затем из DI удаляется <c>IBusInstance</c> (MassTransit),
	/// чтобы MassTransit не запускал её и не ждал подключения к брокеру.</item>
	/// </list>
	/// После перехода на один vhost RabbitMQ класс, <see cref="INotificationsBus"/> и <see cref="NotificationsBusInstance"/>
	/// удаляются: уведомления публикуются обычным IPublishEndpoint основной шины
	/// </summary>
	public static class NotificationBusRegistrationExtensions
	{
		/// <summary>
		/// Адрес-заглушка (такого хоста нет): шина не подключается к брокеру, хост нужен MassTransit, чтобы сформировать
		/// сообщение. В строки outbox попадает адрес вида rabbitmq://notifications-outbox/{exchange}. Сервер RabbitMQ и vhost
		/// из адреса назначения при отправке не используются — служба отправки outbox передаёт сообщение в exchange
		/// с тем же именем через своё подключение (vhost notifications). Ответить на такое сообщение нельзя:
		/// адрес отправителя тоже фиктивный
		/// </summary>
		private const string _outboxOnlyHost = "notifications-outbox";

		/// <summary>
		/// Вторая шина приложения для vhost'а уведомлений. Не запускается и не подключается к брокеру:
		/// используется только для записи сообщений в outbox (таблицы notifications_*),
		/// отправляет их воркер уведомлений (для него этот vhost основной).
		/// Публикация — через <see cref="IUnitOfWorkOutbox{TBus}"/> в транзакции UoW. Повторный вызов ничего не делает.
		/// </summary>
		public static IServiceCollection AddNotificationBusNotHosted(this IServiceCollection services)
		{
			if(services.Any(x => x.ServiceType == typeof(INotificationsBus)))
			{
				return services;
			}

			// Маппинг ключей дедупликации уведомлений (их пишет публикатор уведомлений)
			services.AddMappingAssemblies(typeof(NotificationDeduplicationKeyMap).Assembly);

			var busInstancesBefore = services
				.Where(x => x.ServiceType == typeof(IBusInstance))
				.ToList();

			var busConfigurator = new ServiceCollectionBusConfigurator<INotificationsBus, NotificationsBusInstance>(services);

			services.AddExplicitUnitOfWorkOutbox<INotificationsBus>(OutboxBusNames.Notifications);

			busConfigurator.UsingRabbitMq((context, rabbitCfg) => rabbitCfg.Host(_outboxOnlyHost, "/", _ => { }));

			busConfigurator.Complete();

			// Регистрацию IBusInstance убираем: так MassTransit не запускает эту шину при старте сервиса и не ждёт её подключения к брокеру
			foreach(var busInstance in services
				.Where(x => x.ServiceType == typeof(IBusInstance) && !busInstancesBefore.Contains(x))
				.ToList())
			{
				services.Remove(busInstance);
			}

			return services;
		}
	}
}

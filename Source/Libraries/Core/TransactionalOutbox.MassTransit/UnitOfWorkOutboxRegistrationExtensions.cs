using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using MassTransit.DependencyInjection;
using QS.Project.Core;
using TransactionalOutbox.MassTransit.Mapping;

namespace TransactionalOutbox.MassTransit
{
	public static class UnitOfWorkOutboxRegistrationExtensions
	{
		/// <summary>
		/// <see cref="IUnitOfWorkOutbox{TBus}"/>: публикация в транзакцию переданного UoW.
		/// Сообщения пишутся в таблицы шины <paramref name="busName"/>, отправляют их воркеры этого vhost.
		/// На бэкенде для основной шины регистрируется в AddVodovozOutbox, отдельно вызывается на десктопе
		/// и для второй шины приложения (другой vhost RabbitMQ).
		/// После перехода на один vhost RabbitMQ вторых шин не будет: отдельно вызывается только на десктопе
		/// </summary>
		/// <param name="busName">Логическое имя шины, см. <see cref="OutboxBusNames"/></param>
		/// <remarks>
		/// Для основной шины (<see cref="IBus"/>), если IPublishEndpoint из DI не связан с UoW scope (десктоп),
		/// заодно запрещает публикацию через IPublishEndpoint / ISendEndpointProvider (MassTransit) из DI вне потребителя,
		/// чтобы сообщение нельзя было случайно отправить мимо outbox (см. <see cref="ExplicitOutboxOnlyScopedBusContextProvider"/>).
		/// Вызывать после регистрации шины (AddMassTransit).
		/// </remarks>
		public static IServiceCollection AddExplicitUnitOfWorkOutbox<TBus>(this IServiceCollection services, string busName)
			where TBus : class, IBus
		{
			services.AddSingleton<IUnitOfWorkOutbox<TBus>>(serviceProvider =>
				new UnitOfWorkOutbox<TBus>(serviceProvider.GetRequiredService<TBus>(), busName));
			// Маппинги NHibernate таблиц outbox (папка Mapping): сообщения в них пишет NHibernate
			services.AddMappingAssemblies(typeof(OutboxStateRecordMap<>).Assembly);

			if(typeof(TBus) == typeof(IBus) && !IsPublishEndpointBoundToUnitOfWork(services))
			{
				services.Replace(ServiceDescriptor.Scoped<IScopedBusContextProvider<IBus>, ExplicitOutboxOnlyScopedBusContextProvider>());
			}

			return services;
		}

		/// <summary>
		/// IPublishEndpoint из DI уже пишет в UoW scope (бэкенд, AddVodovozOutbox)
		/// </summary>
		private static bool IsPublishEndpointBoundToUnitOfWork(IServiceCollection services) =>
			services.LastOrDefault(x => x.ServiceType == typeof(IScopedBusContextProvider<IBus>))?.ImplementationType
				== typeof(UnitOfWorkScopedBusContextProvider);
	}
}

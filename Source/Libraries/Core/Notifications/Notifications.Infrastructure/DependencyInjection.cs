using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Contracts;

namespace Notifications.Infrastructure
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Регистрирует публикатор пуш-уведомлений с резервной отправкой СМС 
		/// <see cref="MappingOutboxNotificationWithSmsFallbackPublisher{TDomainEvent}"/>.
		/// <para>
		/// Вызывающий код должен дополнительно зарегистрировать:
		/// <see cref="IIntegrationEventBuilder{TDomainEvent, TIntegrationEvent}"/>,
		/// <see cref="IOutboxSettingsProvider{TEvent}"/>,
		/// <see cref="ISmsNotificationSendingPolicy"/>
		/// и создатели смс уведомлений <see cref="ISmsNotificationCreator{TDomainEvent}"/>
		/// </para>
		/// </summary>
		/// <typeparam name="TDomainEvent">Тип доменного события</typeparam>
		/// <typeparam name="TIntegrationEvent">Тип интеграционного события</typeparam>
		public static IServiceCollection AddMappingOutboxNotificationWithSmsFallbackPublisher<TDomainEvent, TIntegrationEvent>(
			this IServiceCollection services)
			where TDomainEvent : IIdempotentOutboxMessage
		{
			services.AddScoped<MappingOutboxNotificationPublisher<TDomainEvent, TIntegrationEvent>>();

			services.AddScoped<IOutboxNotificationPublisher<TDomainEvent>>(
				sp => new MappingOutboxNotificationWithSmsFallbackPublisher<TDomainEvent>(
					sp.GetRequiredService<ILogger<MappingOutboxNotificationWithSmsFallbackPublisher<TDomainEvent>>>(),
					sp.GetRequiredService<MappingOutboxNotificationPublisher<TDomainEvent, TIntegrationEvent>>(),
					sp.GetRequiredService<IOutboxSettingsProvider<TDomainEvent>>(),
					sp.GetRequiredService<ISmsNotificationSendingPolicy>(),
					sp.GetRequiredService<IEnumerable<ISmsNotificationCreator<TDomainEvent>>>()));

			return services;
		}
	}
}

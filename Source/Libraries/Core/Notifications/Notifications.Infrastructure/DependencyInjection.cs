using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Notifications.Core;

namespace Notifications.Infrastructure
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Регистрирует публикатор пуш-уведомлений с резервной отправкой СМС 
		/// <see cref="SmsFallbackNotificationPublisher{TDomainEvent}"/>.
		/// <para>
		/// Вызывающий код должен дополнительно зарегистрировать:
		/// <see cref="IIntegrationEventBuilder{TDomainEvent, TIntegrationEvent}"/>,
		/// <see cref="INotificationSettingsProvider{TEvent}"/>,
		/// <see cref="ISmsNotificationSendingPolicy"/>
		/// и создатели смс уведомлений <see cref="ISmsNotificationCreator{TDomainEvent}"/>
		/// </para>
		/// </summary>
		/// <typeparam name="TDomainEvent">Тип доменного события</typeparam>
		/// <typeparam name="TIntegrationEvent">Тип интеграционного события</typeparam>
		public static IServiceCollection AddNotificationPublisherWithSmsFallback<TDomainEvent, TIntegrationEvent>(
			this IServiceCollection services)
			where TDomainEvent : IDeduplicatedNotification
			where TIntegrationEvent : class
		{
			services.AddScoped<NotificationPublisher<TDomainEvent, TIntegrationEvent>>();

			services.AddNotificationBusNotHosted();

			services.AddScoped<INotificationPublisher<TDomainEvent>>(
				sp => new SmsFallbackNotificationPublisher<TDomainEvent>(
					sp.GetRequiredService<ILogger<SmsFallbackNotificationPublisher<TDomainEvent>>>(),
					sp.GetRequiredService<NotificationPublisher<TDomainEvent, TIntegrationEvent>>(),
					sp.GetRequiredService<INotificationSettingsProvider<TDomainEvent>>(),
					sp.GetRequiredService<ISmsNotificationSendingPolicy>(),
					sp.GetRequiredService<IEnumerable<ISmsNotificationCreator<TDomainEvent>>>()));

			return services;
		}
	}
}

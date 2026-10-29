
using EdoNotifications.Application.Factories;
using EdoNotifications.Application.Providers;
using EdoNotifications.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure;
using Notifications.Core;

public static class DependencyInjection
{
	public static IServiceCollection AddEdoNotificationsSettingsProvider(
		this IServiceCollection services)
	{
		services.AddSingleton<IEdoNotificationsSettingsProvider, EdoNotificationsSettingsProvider>();

		services.AddScoped<INotificationSettingsProvider<EdoNotificationMessage>>(
			sp => sp.GetRequiredService<IEdoNotificationsSettingsProvider>());

		return services;
	}

	public static IServiceCollection AddEdoNotifications(
		this IServiceCollection services)
	{
		services.AddEdoNotificationsSettingsProvider();
		services.AddScoped<IEdoNotificationMessageFactory, EdoNotificationMessageFactory>();
		// Уведомления сохраняются в outbox MassTransit (таблицы notifications_*) в транзакции UoW,
		// отправляет их воркер уведомлений
		services.AddScoped<IIntegrationEventBuilder<EdoNotificationMessage, EdoNotificationMessage>, SameEventBuilder<EdoNotificationMessage>>();
		services.AddScoped<INotificationPublisher<EdoNotificationMessage>, NotificationPublisher<EdoNotificationMessage, EdoNotificationMessage>>();
		services.AddNotificationBusNotHosted();

		return services;
	}
}

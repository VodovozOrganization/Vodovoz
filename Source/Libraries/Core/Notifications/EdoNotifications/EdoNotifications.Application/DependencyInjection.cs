
using EdoNotifications.Application.Factories;
using EdoNotifications.Application.Providers;
using EdoNotifications.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure;
using TransactionalOutbox.Abstractions;
using VodovozInfrastructure.Cryptography;

public static class DependencyInjection
{
	public static IServiceCollection AddEdoNotificationsSettingsProvider(
		this IServiceCollection services)
	{
		services.AddSingleton<IEdoNotificationsSettingsProvider, EdoNotificationsSettingsProvider>();

		services.AddScoped<IOutboxSettingsProvider<EdoNotificationMessage>>(
			sp => sp.GetRequiredService<IEdoNotificationsSettingsProvider>());

		return services;
	}

	public static IServiceCollection AddEdoNotifications(
		this IServiceCollection services)
	{
		services.AddEdoNotificationsSettingsProvider();
		services.AddScoped<IEdoNotificationMessageFactory, EdoNotificationMessageFactory>();
		services.AddScoped<IOutboxNotificationPublisher<EdoNotificationMessage>, DirectOutboxNotificationPublisher<EdoNotificationMessage>>();
		services.AddScoped<IMD5HexHashFromString,  MD5HexHashFromString>();

		return services;
	}
}
